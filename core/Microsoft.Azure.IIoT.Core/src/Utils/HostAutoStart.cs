// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using Microsoft.Extensions.Logging;
    using Autofac;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Host auto starter
    /// </summary>
    public sealed class HostAutoStart : IDisposable, IStartable {

        /// <summary>
        /// Create host auto starter
        /// </summary>
        /// <param name="hosts"></param>
        /// <param name="logger"></param>
        public HostAutoStart(IEnumerable<IHostProcess> hosts, ILogger logger) {
            _hosts = hosts?.ToList() ?? throw new ArgumentNullException(nameof(hosts));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void Start() {
            StartAsync().Wait();
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <returns></returns>
        private async Task StopAsync() {
            try {
                _logger.LogDebug("Stopping all hosts...");
                foreach (var host in _hosts.Select(h => h).Reverse()) {
                    await host.StopAsync().ConfigureAwait(false);
                }
                _logger.LogInformation("All hosts stopped.");
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to stop all hosts.");
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        private async Task StartAsync() {
            try {
                _logger.LogDebug("Starting all hosts...");
                foreach (var host in _hosts) {
                    await host.StartAsync().ConfigureAwait(false);
                }
                _logger.LogInformation("All hosts started.");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start some hosts.");
                throw;
            }
        }

        private readonly List<IHostProcess> _hosts;
        private readonly ILogger _logger;
    }
}