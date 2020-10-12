// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Default;
    using Autofac;

    /// <summary>
    /// Injected registry event handlers
    /// </summary>
    public class RegistryEventHandlers : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<EndpointEventBroker>()
                .AsImplementedInterfaces();
            builder.RegisterType<ApplicationEventBroker>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
