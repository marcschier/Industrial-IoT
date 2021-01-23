// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Default;
    using Autofac;

    /// <summary>
    /// Injected registry event brokers
    /// </summary>
    public class DiscoveryEventBrokers : Module {

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
