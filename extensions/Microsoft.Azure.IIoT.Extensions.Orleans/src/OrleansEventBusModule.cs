// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans {
    using Microsoft.Azure.IIoT.Extensions.Orleans.Clients;
    using Microsoft.Azure.IIoT.Extensions.Orleans.Services;
    using Microsoft.Azure.IIoT.Messaging.Services;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Tasks.Services;
    using Microsoft.Azure.IIoT.Tasks;
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
                .IfNotRegistered(typeof(IEventBus));

            builder.RegisterType<EventBusHost>().AsSelf()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEventBus));
            builder.RegisterType<OrleansEventBusClient>()
                .AsImplementedInterfaces().InstancePerDependency()
                .IfNotRegistered(typeof(IEventBus));

            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));
            base.Load(builder);
        }
    }
}
