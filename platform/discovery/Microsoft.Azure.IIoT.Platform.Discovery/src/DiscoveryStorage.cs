// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery {
    using Microsoft.Azure.IIoT.Platform.Discovery.Services;
    using Microsoft.Azure.IIoT.Platform.Discovery.Storage;
    using Autofac;

    /// <summary>
    /// Injected registry control services
    /// </summary>
    public class DiscoveryStorage : DiscoveryEventBrokers {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Services
            builder.RegisterType<EndpointRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ApplicationRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<EndpointDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEndpointRepository));
            builder.RegisterType<ApplicationDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IApplicationRepository));

            base.Load(builder);
        }
    }
}
