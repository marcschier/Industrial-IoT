// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Platform.Registry.Default;
    using Autofac;

    /// <summary>
    /// Injected registry services
    /// </summary>
    public sealed class RegistryServices : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Services
            builder.RegisterType<EndpointEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<EndpointRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<ApplicationEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ApplicationRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<SupervisorEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SupervisorRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<PublisherEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<PublisherRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<DiscovererEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DiscovererRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<GatewayEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<GatewayRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
