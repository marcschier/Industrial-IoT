// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans.Grains {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using Autofac.Extensions.DependencyInjection;
    using Autofac;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Reflection;
    using global::Orleans;
    using global::Orleans.Configuration;
    using global::Orleans.Hosting;

    /// <summary>
    /// Client host
    /// </summary>
    public class OrleansClientHost : IOrleansClientHost, IOrleansGrainClient {

        /// <inheritdoc/>
        public IClusterClient Client { get; private set; }

        /// <inheritdoc/>
        public IGrainFactory Grains => Client;

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="scope"></param>
        /// <param name="config"></param>
        /// <param name="injector"></param>
        /// <param name="identity"></param>
        public OrleansClientHost(ILogger logger, ILifetimeScope scope, 
            IInjector injector = null, IOrleansConfig config = null,
            IProcessIdentity identity = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _config = config;
            _identity = identity;
            _injector = injector;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            try {
                if (Client != null) {
                    throw new ResourceInvalidStateException(
                        "Client host already started");
                }
                // define the cluster configuration
                var builder = ConfigureBuilder(new ClientBuilder())
                    .UseServiceProviderFactory(
                        new AutofacChildLifetimeScopeServiceProviderFactory(
                            _scope, builder => _injector?.Inject(builder)))
                    .ConfigureLogging(logging => logging.AddConsole().AddDebug())
                    .ConfigureApplicationParts(parts => {
                        parts
                            .AddApplicationPart(Assembly.GetEntryAssembly())
                            .AddApplicationPart(Assembly.GetExecutingAssembly())
                            .WithReferences();
                    })
                    ;
                
                var client = builder.Build();
                await client.Connect().ConfigureAwait(false);
                Client = client;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start client host");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
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

        /// <summary>
        /// Configure the builder
        /// </summary>
        /// <returns></returns>
        protected IClientBuilder ConfigureBuilder(IClientBuilder builder) {
            return builder
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = _config?.ClusterId;
                    options.ServiceId = _config?.ServiceId ?? _identity?.ServiceId;
                })
                .UseLocalhostClustering()
                .Configure<EndpointOptions>(options =>
                    options.AdvertisedIPAddress = IPAddress.Loopback);
        }

        private readonly ILogger _logger;
        private readonly ILifetimeScope _scope;
        private readonly IOrleansConfig _config;
        private readonly IProcessIdentity _identity;
        private readonly IInjector _injector;
    }
}