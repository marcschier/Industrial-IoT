// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus {
    using Microsoft.Azure.IIoT.Azure.ServiceBus.Clients;
    using Microsoft.Azure.IIoT.Azure.ServiceBus.Services;
    using Microsoft.Azure.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.Azure.IIoT.Messaging.Services;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Tasks.Services;
    using Microsoft.Azure.IIoT.Tasks;
    using Autofac;

    /// <summary>
    /// Injected service bus
    /// </summary>
    public class ServiceBusEventBusSupport : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register event bus for integration events
            builder.RegisterType<EventBusHost>().AsSelf()
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ServiceBusClientFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServiceBusEventBus>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(IEventBus));
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));

            builder.RegisterType<ServiceBusConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServiceBusEventBusConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}