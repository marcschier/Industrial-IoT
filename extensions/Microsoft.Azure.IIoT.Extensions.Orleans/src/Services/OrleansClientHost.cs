// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Services {
    using Microsoft.Azure.IIoT.Extensions.Orleans;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Autofac.Extensions.DependencyInjection;
    using Autofac;
    using System;
    using System.Threading.Tasks;
    using global::Orleans;
    using global::Orleans.Hosting;

    /// <summary>
    /// Orleans injectable client
    /// </summary>
    public sealed class OrleansClientHost : LocalHostCluster, IOrleansClientHost,
        IOrleansGrainClient, IHostProcess, IDisposable {

        /// <inheritdoc/>
        public IClusterClient Client => _host.Client;

        /// <inheritdoc/>
        public IGrainFactory Grains => _host.Grains;

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="scope"></param>
        /// <param name="injector"></param>
        public OrleansClientHost(ILogger logger, ILifetimeScope scope,
            IInjector injector = null) {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _injector = injector;
            _host = new OrleansClientService<OrleansClientHost>(logger, this);
        }

        /// <inheritdoc/>
        public Task StartAsync() {
            return _host.StartAsync(default);
        }

        /// <inheritdoc/>
        public Task StopAsync() {
            return _host.StopAsync(default);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _host.Dispose();
        }

        /// <inheritdoc/>
        internal override void ConfigureClient(IClientBuilder builder) {
            builder
                .ConfigureServices(services => Configure(services))
                .ConfigureLogging(logging => Configure(logging))
                .ConfigureApplicationParts(parts => Configure(parts))
                .UseServiceProviderFactory(
                    new AutofacChildLifetimeScopeServiceProviderFactory(
                        _scope, builder => _injector?.Inject(builder)))
                ;
            Configure(builder);
        }

        private readonly ILifetimeScope _scope;
        private readonly IInjector _injector;
        private readonly OrleansClientService<OrleansClientHost> _host;
    }
}