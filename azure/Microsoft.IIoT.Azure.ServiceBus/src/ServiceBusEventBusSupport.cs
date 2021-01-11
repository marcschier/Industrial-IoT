// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus {
    using Microsoft.IIoT.Azure.ServiceBus.Clients;
    using Microsoft.IIoT.Azure.ServiceBus.Services;
    using Microsoft.IIoT.Azure.ServiceBus.Runtime;
    using Microsoft.IIoT.Extensions.Messaging.Services;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Tasks.Services;
    using Microsoft.IIoT.Extensions.Tasks;
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
                .IfNotRegistered(typeof(IEventBusSubscriber))
                .IfNotRegistered(typeof(IEventBusPublisher));
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));

            builder.AddOptions();
            builder.RegisterType<ServiceBusConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<ServiceBusEventBusConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
