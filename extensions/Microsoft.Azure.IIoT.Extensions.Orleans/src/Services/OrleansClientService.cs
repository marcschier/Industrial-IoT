// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Services {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using global::Orleans;

    /// <summary>
    /// Client host
    /// </summary>
    public sealed class OrleansClientService<TStartup> : IHostedService, IOrleansClientHost,
        IOrleansGrainClient, IDisposable where TStartup : OrleansStartup {

        /// <inheritdoc/>
        public IClusterClient Client { get; private set; }

        /// <inheritdoc/>
        public IGrainFactory Grains => Client;

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="startup"></param>
        public OrleansClientService(ILogger logger, TStartup startup = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _startup = startup ?? (TStartup)Activator.CreateInstance(typeof(TStartup));
           // _register = startup != null;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken ct) {
            try {
                if (Client != null) {
                    throw new ResourceInvalidStateException(
                        "Client host already started");
                }

                var clientBuilder = new ClientBuilder();
                _startup.ConfigureClient(clientBuilder);
                var client = clientBuilder
                    .ConfigureServices(services => {
                     //   services.TryAddSingleton<IOrleansGrainClient>(this);
                    })
                    .Build();
                await client.Connect().ConfigureAwait(false);
                Client = client;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start client host");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken ct) {
            try {
                await Client.Close().ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop client host");
                throw;
            }
            finally {
                Client.Dispose();
                Client = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(() => StopAsync(default)).Wait();
        }

        private readonly ILogger _logger;
        private readonly TStartup _startup;
    }
}