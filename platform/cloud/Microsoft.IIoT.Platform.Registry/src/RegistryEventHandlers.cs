// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry {
    using Microsoft.IIoT.Platform.Registry.Handlers;
    using Microsoft.IIoT.Platform.Registry.Default;
    using Autofac;

    /// <summary>
    /// Injected registry event handlers
    /// </summary>
    public sealed class RegistryEventHandlers : Module {

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
