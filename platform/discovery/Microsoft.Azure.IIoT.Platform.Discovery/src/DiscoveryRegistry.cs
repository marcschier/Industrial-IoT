// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery {
    using Microsoft.Azure.IIoT.Platform.Discovery.Services;
    using Microsoft.Azure.IIoT.Platform.Discovery.Clients;
    using Autofac;

    /// <summary>
    /// Injected registry services
    /// </summary>
    public sealed class DiscoveryRegistry : DiscoveryStorage {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<DiscoveryRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDiscoveryServices));
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
