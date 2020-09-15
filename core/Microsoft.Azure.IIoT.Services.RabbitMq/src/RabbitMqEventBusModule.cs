// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq {
    using Microsoft.Azure.IIoT.Services.RabbitMq.Clients;
    using Microsoft.Azure.IIoT.Messaging.Default;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Tasks;
    using Autofac;

    /// <summary>
    /// Injected RabbitMq bus
    /// </summary>
    public class RabbitMqEventBusModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<EventBusHost>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqConnection>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqEventBus>()
                .AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));

            base.Load(builder);
        }
    }
}
