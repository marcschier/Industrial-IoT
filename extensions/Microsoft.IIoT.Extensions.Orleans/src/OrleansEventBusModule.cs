// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans {
    using Microsoft.IIoT.Extensions.Orleans.Clients;
    using Microsoft.IIoT.Extensions.Orleans.Runtime;
    using Microsoft.IIoT.Extensions.Orleans.Services;
    using Microsoft.IIoT.Extensions.Messaging.Services;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Tasks.Services;
    using Microsoft.IIoT.Extensions.Tasks;
    using Autofac;

    /// <summary>
    /// Injected orleans event bus
    /// </summary>
    public class OrleansEventBusModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            //
            // Generic grain client - might already be injected
            // if running in test cluster or in a hosted service
            // therefore we check to not override.
            //
            builder.RegisterType<OrleansClientHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansGrainClient))
                .IfNotRegistered(typeof(IEventBusSubscriber))
                .IfNotRegistered(typeof(IEventBusPublisher));
            builder.RegisterType<OrleansClusterConfig>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(IEventBusSubscriber))
                .IfNotRegistered(typeof(IEventBusPublisher));

            builder.RegisterType<EventBusHost>().AsSelf()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEventBusSubscriber))
                .IfNotRegistered(typeof(IEventBusPublisher));
            builder.RegisterType<OrleansEventBusClient>()
                .AsImplementedInterfaces().InstancePerDependency()
                .IfNotRegistered(typeof(IEventBusSubscriber))
                .IfNotRegistered(typeof(IEventBusPublisher));

            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));
            base.Load(builder);
        }
    }
}
