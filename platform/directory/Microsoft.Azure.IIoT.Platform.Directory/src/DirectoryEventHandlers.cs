// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory {
    using Microsoft.Azure.IIoT.Platform.Directory.Handlers;
    using Microsoft.Azure.IIoT.Platform.Directory.Default;
    using Autofac;

    /// <summary>
    /// Injected registry event handlers
    /// </summary>
    public sealed class DirectoryEventHandlers : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<SupervisorTwinEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<SupervisorEventBroker>()
                .AsImplementedInterfaces();

            builder.RegisterType<PublisherTwinEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherEventBroker>()
                .AsImplementedInterfaces();

            builder.RegisterType<DiscovererTwinEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscovererEventBroker>()
                .AsImplementedInterfaces();

            builder.RegisterType<GatewayTwinEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<GatewayEventBroker>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
