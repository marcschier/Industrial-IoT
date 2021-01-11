// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Services {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Utils;
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
        public Task StartAsync(CancellationToken ct) {
            try {
                if (_host != null) {
                    throw new ResourceInvalidStateException(
                        "Silo Host already started");
                }
                var hostBuilder = new HostBuilder();
                _startup.ConfigureHost(hostBuilder);
                var host = hostBuilder
                    .UseOrleans(builder => {
                        _startup.ConfigureSilo(builder);
                    })
                    .ConfigureServices(services => {
                        services.TryAddSingleton<IOrleansSiloHost>(this);
                    })
                    .Build();

                // Start and return
                _cts = new CancellationTokenSource();
                _run = Task.Run(() => host.StartAsync(_cts.Token), ct);
                _host = host;
                return Task.CompletedTask;
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
                if (_run != null) {
                    _cts.Cancel();
                    await _run.ConfigureAwait(false);
                    _run = null;
                }
                await _host.StopAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop silo host");
                throw;
            }
            finally {
                _host?.Dispose();
                _host = null;

                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(() => StopAsync(default)).Wait();
        }

        private readonly ILogger _logger;
        private readonly TStartup _startup;
        private Task _run;
        private IHost _host;
        private CancellationTokenSource _cts;
    }
}