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
    using Microsoft.Azure.IIoT.Exceptions;

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
            _logger.LogDebug("Stopping all hosts...");
            foreach (var host in _hosts.Select(h => h).Reverse()) {
                try {
                    await host.StopAsync().ConfigureAwait(false);
                }
                catch (Exception ex) {
                    _logger.LogWarning(ex, "Failed to stop a host of type {type}...",
                        host.GetType().Name);
                }
            }
            _logger.LogInformation("All hosts stopped.");
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <returns></returns>
        private async Task StartAsync() {
            var exceptions = new List<Exception>();
            _logger.LogDebug("Starting all hosts...");
            var hosts = new Queue<IHostProcess>(_hosts);
            while (true) {
                var count = hosts.Count;
                if (count == 0) {
                    // No more hosts to start
                    _logger.LogInformation("All hosts started.");
                    return;
                }
                for (var i = 0; i < count; i++) {
                    var host = hosts.Dequeue();
                    try {
                        await host.StartAsync().ConfigureAwait(false);
                    }
                    catch (ResourceInvalidStateException rex) {
                        // Already started.
                        _logger.LogWarning(rex, 
                            "Tried to start {type} but was already started.",
                            host.GetType().Name);
                    }
                    catch (Exception ex) {
                        _logger.LogWarning(ex, "Failed to start a host of type {type}.",
                            host.GetType().Name);
                        exceptions.Add(ex);
                        hosts.Enqueue(host); // start later
                    }
                }
                if (hosts.Count == count) {
                    // Failed to start remaining - throw
                    if (exceptions.Count > 1) {
                        throw new AggregateException(
                            "Failed to start some hosts", exceptions);
                    }
                    // There must be one.
                    throw exceptions.First();
                }
            }
        }

        private readonly List<IHostProcess> _hosts;
        private readonly ILogger _logger;
    }
}