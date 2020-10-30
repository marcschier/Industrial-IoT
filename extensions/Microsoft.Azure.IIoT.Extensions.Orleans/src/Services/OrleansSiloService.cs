// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Services {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Silo host
    /// </summary>
    public sealed class OrleansSiloService<TStartup> : IOrleansSiloHost,
        IHostedService, IDisposable where TStartup : OrleansStartup {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="startup"></param>
        public OrleansSiloService(ILogger logger, TStartup startup = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _startup = startup ?? (TStartup)Activator.CreateInstance(typeof(TStartup));
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken ct) {
            try {
                if (_host != null) {
                    throw new ResourceInvalidStateException(
                        "Silo Host already started");
                }
                var hostBuilder = new HostBuilder();
                _startup.ConfigureHost(hostBuilder);
                var host = hostBuilder
                    .UseOrleans(builder => _startup.ConfigureSilo(builder))
                    .ConfigureServices(services => {
                        services.TryAddSingleton<IOrleansSiloHost>(this);
                    })
                    .Build();
                await host.StartAsync().ConfigureAwait(false);
                _host = host;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start silo host");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken ct) {
            try {
                if (_host == null) {
                    return;
                }
                await _host.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop silo host");
                throw;
            }
            finally {
                //  await Try.Async(() => SiloHost.DisposeAsync()).ConfigureAwait(false);
                _host?.Dispose();
                _host = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(() => StopAsync(default)).Wait();
        }

        private readonly ILogger _logger;
        private readonly TStartup _startup;
        private IHost _host;
    }
}