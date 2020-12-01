// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Edge.Tunnel.Service {
    using Microsoft.IIoT.Platform.Edge.Tunnel.Service.Runtime;
    using Microsoft.IIoT.Hosting;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generic host service which manages IHostProcess objects.
    /// </summary>
    public class HostStarterService : IHostedService {

        /// <summary>
        /// Details of application hosting environment.
        /// </summary>
        public IHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// Handler for subscribing to application lifetime events.
        /// </summary>
        public IHostApplicationLifetime HostApplicationLifetime { get; }

        /// <summary>
        /// Runtime configuration, will be provided by DI.
        /// </summary>
        public HostingOptions Config { get; }

        /// <summary>
        /// Service information, will be provided by DI.
        /// </summary>
        public IProcessIdentity ServiceInfo { get; }

        /// <summary>
        /// List of IHostProcess objects that will be managed by this instance, provided by DI.
        /// </summary>
        private readonly List<IHostProcess> _hostProcesses;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for generic host service which manages IHostProcess objects.
        /// </summary>
        /// <param name="hostEnvironment"></param>
        /// <param name="hostApplicationLifetime"></param>
        /// <param name="config"></param>
        /// <param name="serviceInfo"></param>
        /// <param name="hostProcesses"></param>
        /// <param name="logger"></param>
        public HostStarterService(
            IHostEnvironment hostEnvironment,
            IHostApplicationLifetime hostApplicationLifetime,
            HostingOptions config,
            IProcessIdentity serviceInfo,
            IEnumerable<IHostProcess> hostProcesses,
            ILogger logger
        ) {
            HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
            HostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ServiceInfo = serviceInfo ?? throw new ArgumentNullException(nameof(serviceInfo));
            _hostProcesses = hostProcesses?.ToList() ?? throw new ArgumentNullException(nameof(hostProcesses));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken) {
            try {
                _logger.LogDebug("Starting all hosts...");
                await Task.WhenAll(_hostProcesses.Select(h => h.StartAsync())).ConfigureAwait(false);
                _logger.LogInformation("All hosts started.");

                // Print some useful information at bootstrap time
                _logger.LogInformation("{service} service started with id {id}",
                    ServiceInfo.Name, ServiceInfo.Id);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start some hosts.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) {
            try {
                _logger.LogDebug("Stopping all hosts...");
                await Task.WhenAll(_hostProcesses.Select(h => h.StopAsync())).ConfigureAwait(false);
                _logger.LogInformation("All hosts stopped.");
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to stop all hosts.");
                throw;
            }
        }
    }
}
