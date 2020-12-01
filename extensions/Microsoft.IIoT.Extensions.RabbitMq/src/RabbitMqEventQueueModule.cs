// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq {
    using Microsoft.IIoT.Extensions.RabbitMq.Clients;
    using Microsoft.IIoT.Extensions.RabbitMq.Runtime;
    using Autofac;

    /// <summary>
    /// Injected RabbitMq queue client
    /// </summary>
    public class RabbitMqEventQueueModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<RabbitMqQueueClient>()
                .AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<RabbitMqConnection>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqHealthCheck>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
