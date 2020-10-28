// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans.Grains {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Logging;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Reflection;
    using global::Orleans;
    using global::Orleans.Configuration;
    using global::Orleans.Hosting;

    /// <summary>
    /// Silo host
    /// </summary>
    public class OrleansSiloHost : IOrleansSiloHost, IHostProcess {

        /// <inheritdoc/>
        public ISiloHost SiloHost { get; set; }

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="scope"></param>
        /// <param name="config"></param>
        /// <param name="injector"></param>
        /// <param name="identity"></param>
        public OrleansSiloHost(ILogger logger, ILifetimeScope scope, IOrleansConfig config,
            IInjector injector = null, IProcessIdentity identity = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _identity = identity;
            _injector = injector;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            try {
                if (SiloHost != null) {
                    throw new ResourceInvalidStateException(
                        "Silo Host already started");
                }
                // define the cluster configuration
                var builder = ConfigureBuilder(new SiloHostBuilder())
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

                var host = builder.Build();
                await host.StartAsync().ConfigureAwait(false);
                SiloHost = host;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start silo host");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            try {
                await SiloHost.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to stop silo host");
                throw;
            }
            finally {
              //  await Try.Async(() => SiloHost.DisposeAsync()).ConfigureAwait(false);
                SiloHost.Dispose();
                SiloHost = null;
            }
        }

        /// <summary>
        /// Configure the builder
        /// </summary>
        /// <returns></returns>
        protected ISiloHostBuilder ConfigureBuilder(ISiloHostBuilder builder) {
            return builder
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = _config.ClusterId;
                    options.ServiceId = _config.ServiceId ?? _identity?.ServiceId;
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