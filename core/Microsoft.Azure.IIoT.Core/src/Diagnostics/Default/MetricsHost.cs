// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics.Default {
    using Microsoft.Azure.IIoT.Diagnostics;
    using Serilog;
    using Prometheus;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Start and stop metrics collection
    /// </summary>
    public class MetricsHost : IHostProcess {

        /// <summary>
        /// Auto registers metric server
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public MetricsHost(IEnumerable<IMetricsHandler> handlers,
            ILogger logger, IMetricServerConfig config = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config;
            _handlers = handlers?.ToList();
        }

        /// <inheritdoc/>
        public Task StartAsync() {
            if (_server == null) {
                _server = Create();
                if (_server == null) {
                    _logger.Information("Metrics collection is disabled.");
                }
                else {
                    try {
                        _server.Start();
                        _logger.Information("Metric server started.");
                    }
                    catch (Exception ex) {
                        _server = Create(false);
                        if (_server != null) {
                            try {
                                _server.Start();
                                return Task.CompletedTask;
                            }
                            catch {
                                // Fail
                            }
                        }
                        _logger.Error(ex, "Failed to start metrics server.");
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            if (_server != null) {
                await _server.StopAsync();
                _server = null;
                _logger.Information("Metric server stopped.");
            }
        }

        /// <summary>
        /// Create metrics server
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>;
        protected virtual IMetricServer CreateServer(IMetricServerConfig config) {
            return new MetricServer(config.Port, config.Path ?? "metrics/",
                null, config.UseHttps);
        }

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="noServer"></param>
        /// <returns></returns>
        private IMetricServer Create(bool noServer = false) {
            if (_config == null) {
                return null;
            }
            if (!_config.DiagnosticsLevel.HasFlag(DiagnosticsLevel.Disabled)) {
                if (_config.DiagnosticsLevel.HasFlag(DiagnosticsLevel.PushMetrics) ||
                    _config.Port == 0) {
                    if (_handlers != null && _handlers.Count != 0) {
                        // Use push collector
                        return new MetricsCollector(_handlers, _config, _logger);
                    }
                }
                else if (!noServer) {
                    return CreateServer(_config);
                }
            }
            return null;
        }


        private IMetricServer _server;
        private readonly ILogger _logger;
        private readonly IMetricServerConfig _config;
        private readonly List<IMetricsHandler> _handlers;
    }
}
