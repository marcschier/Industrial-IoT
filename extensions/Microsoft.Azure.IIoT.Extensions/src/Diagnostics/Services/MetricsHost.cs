// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics.Services {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Prometheus;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.IIoT.Utils;

    /// <summary>
    /// Start and stop metrics collection
    /// </summary>
    public class MetricsHost : IHostProcess, IDisposable {

        /// <summary>
        /// Auto registers metric server
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MetricsHost(IEnumerable<IMetricsHandler> handlers,
            ILogger logger, IOptions<MetricsServerOptions> options) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _handlers = handlers?.ToList();
        }

        /// <inheritdoc/>
        public Task StartAsync() {
            if (_server == null) {
                SetServer();
                if (_server == null) {
                    _logger.LogInformation("Metrics collection is disabled.");
                }
                else {
                    try {
                        _server.Start();
                        _logger.LogInformation("Metric server started.");
                    }
                    catch (Exception ex) {
                        SetServer(false);
                        if (_server != null) {
                            try {
                                _server.Start();
                                return Task.CompletedTask;
                            }
                            catch {
                                _server.Dispose();
                                _server = null;
                            }
                        }
                        _logger.LogError(ex, "Failed to start metrics server.");
                        throw;
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            if (_server != null) {
                await _server.StopAsync().ConfigureAwait(false);
                _server.Dispose();
                _server = null;
                _logger.LogInformation("Metric server stopped.");
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    Try.Op(() => StopAsync().Wait());
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Create metrics server
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>;
        protected virtual IMetricServer CreateServer(MetricsServerOptions config) {
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            return new MetricServer(config.Port, config.Path ?? "metrics/",
                null, config.UseHttps);
        }

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="noServer"></param>
        /// <returns></returns>
        private void SetServer(bool noServer = false) {
            if (_options == null) {
                return;
            }
            if (!_options.Value.DiagnosticsLevel.HasFlag(DiagnosticsLevel.Disabled)) {
                if (_options.Value.DiagnosticsLevel.HasFlag(DiagnosticsLevel.PushMetrics) ||
                    _options.Value.Port == 0) {
                    if (_handlers != null && _handlers.Count != 0) {
                        // Use push collector
                        _server = new MetricsCollector(_handlers, _options, _logger);
                        return;
                    }
                }
                else if (!noServer) {
                    _server = CreateServer(_options.Value);
                }
            }
        }

        private IMetricServer _server;
        private bool _disposedValue;
        private readonly ILogger _logger;
        private readonly IOptions<MetricsServerOptions> _options;
        private readonly List<IMetricsHandler> _handlers;
    }
}
