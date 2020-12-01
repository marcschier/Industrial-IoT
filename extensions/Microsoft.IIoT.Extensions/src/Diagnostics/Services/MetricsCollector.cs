// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Diagnostics {
    using Microsoft.IIoT.Utils;
    using Prometheus;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Prometheus metrics collector and pusher
    /// </summary>
    public class MetricsCollector : MetricHandler {

        /// <summary>
        /// Create configuration
        /// </summary>
        /// <param name="config"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public MetricsCollector(IEnumerable<IMetricsHandler> handlers,
            IOptions<MetricsServerOptions> config, ILogger logger) : base(null) {
            _handlers = handlers?.ToList() ??
                throw new ArgumentNullException(nameof(handlers));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override Task StartServer(CancellationToken ct) {
            try {
                if (DiagnosticsLevel.NoMetrics !=
                        (_config.Value.DiagnosticsLevel & DiagnosticsLevel.NoMetrics)) {
                    _logger.LogInformation("Starting metrics collector...");
                    _handlers.ForEach(h => h.OnStarting());

                    // Kick off the actual processing to a new thread and return
                    // a Task for the processing thread.
                    return Task.Run(() => RunAsync(ct), ct);
                }
                _logger.LogInformation("Metrics collection is disabled.");
                return Task.CompletedTask;
            }
            catch (Exception ex) {
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Scrape metrics from internal registries and push to collector
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task RunAsync(CancellationToken ct) {
            var duration = Stopwatch.StartNew();
            while (true) {
                duration.Restart();
                try {
                    using (var stream = new MemoryStream()) {
                        await _registry.CollectAndExportAsTextAsync(stream, default).ConfigureAwait(false);

                        foreach (var handler in _handlers) {
                            stream.Position = 0;
                            await Try.Async(() => handler.PushAsync(
                                new NoCloseAdapter(stream), default)).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    _logger.LogDebug(ex, "Failed to collect metrics.");
                }

                var elapsed = duration.Elapsed;
                var interval = _config?.Value.MetricsCollectionInterval ?? kDefaultInterval;

                // Stop only here so that latest state is flushed on exit.
                if (ct.IsCancellationRequested) {
                    break;
                }

                // Wait for interval - todo better use a timer...
                var sleepTime = interval - elapsed;
                if (sleepTime > TimeSpan.Zero) {
                    try {
                        await Task.Delay(sleepTime, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {
                        // Post one more time
                    }
                }
            }
            _handlers.ForEach(h => h.OnStopped());
            _logger.LogInformation("Metrics publishing stopped.");
        }

        private static readonly TimeSpan kDefaultInterval =
#if DEBUG
            TimeSpan.FromSeconds(10);
#else
            TimeSpan.FromMinutes(1);
#endif
        private readonly IOptions<MetricsServerOptions> _config;
        private readonly List<IMetricsHandler> _handlers;
        private readonly ILogger _logger;
    }
}