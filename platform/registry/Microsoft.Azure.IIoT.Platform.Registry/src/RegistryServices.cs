// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Services;
    using Microsoft.Azure.IIoT.Platform.Registry.Storage;
    using Microsoft.Azure.IIoT.Platform.Registry.Clients;
    using Autofac;

    /// <summary>
    /// Injected registry services
    /// </summary>
    public sealed class RegistryServices : RegistryEventHandlers {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Services
            builder.RegisterType<EndpointRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<EndpointDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEndpointRepository));

            builder.RegisterType<ApplicationRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ApplicationDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IApplicationRepository));

            builder.RegisterType<DiscoveryServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DiscoveryProgressLogger>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDiscoveryProgressHandler));
            builder.RegisterType<DiscoveryBulkProcessorAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDiscoveryResultHandler));
            builder.RegisterType<CertificateServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ICertificateServices<string>));

            base.Load(builder);
        }
    }
}
