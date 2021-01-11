// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using Microsoft.IIoT.Extensions.Messaging.Services;
    using Microsoft.IIoT.Extensions.Tasks.Services;
    using Microsoft.IIoT.Extensions.Tasks;
    using Autofac;

    /// <summary>
    /// Injected simple (test) event bus
    /// </summary>
    public class MemoryEventBusModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<EventBusHost>().AsSelf()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SimpleEventBus>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEventBusPublisher));

            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));

            base.Load(builder);
        }
    }
}
