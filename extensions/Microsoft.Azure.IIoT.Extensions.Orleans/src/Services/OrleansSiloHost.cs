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

    /// <summary>
    /// Orleans injectable single silo host
    /// </summary>
    public sealed class OrleansSiloHost : LocalHostCluster, IOrleansSiloHost,
        IHostProcess, IDisposable {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="scope"></param>
        /// <param name="injector"></param>
        public OrleansSiloHost(ILogger logger, ILifetimeScope scope,
            IInjector injector = null) {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _injector = injector;
            _host = new OrleansSiloService<OrleansSiloHost>(logger, this);
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
        internal override void ConfigureHost(IHostBuilder builder) {
            builder
                .UseServiceProviderFactory(
                    new AutofacChildLifetimeScopeServiceProviderFactory(
                        _scope, builder => _injector?.Inject(builder)))
                ;
            Configure(builder);
        }

        private readonly ILifetimeScope _scope;
        private readonly IInjector _injector;
        private readonly OrleansSiloService<OrleansSiloHost> _host;
    }
}