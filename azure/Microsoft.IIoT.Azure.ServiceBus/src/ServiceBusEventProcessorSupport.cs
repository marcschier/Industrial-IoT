// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus {
    using Microsoft.IIoT.Azure.ServiceBus.Clients;
    using Microsoft.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.IIoT.Azure.ServiceBus.Services;
    using Autofac;

    /// <summary>
    /// Injected service bus
    /// </summary>
    public class ServiceBusEventProcessorSupport : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register event bus for integration events
            builder.RegisterType<ServiceBusClientFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServiceBusProcessorHost>()
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ServiceBusConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServiceBusProcessorConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
