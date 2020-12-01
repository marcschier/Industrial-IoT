// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Http.Tunnel {
    using Microsoft.IIoT.Http.Tunnel.Services;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Rpc.Services;
    using Microsoft.IIoT.Rpc;
    using Autofac;

    /// <summary>
    /// Injected http tunnel handler
    /// </summary>
    public sealed class HttpTunnelMethodClient : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterModule<HttpClientModule>();

            //
            // Override default factory with configurable http tunnel
            // method client factory.
            //
            builder.RegisterType<HttpTunnelMethodClientFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<ChunkMethodClient>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(IMethodClient));
        }
    }
}
