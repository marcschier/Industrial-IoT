// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq {
    using Microsoft.IIoT.Extensions.RabbitMq.Services;
    using Microsoft.IIoT.Extensions.RabbitMq.Clients;
    using Microsoft.IIoT.Extensions.RabbitMq.Runtime;
    using Autofac;

    /// <summary>
    /// Injected RabbitMq queue client
    /// </summary>
    public class RabbitMqEventProcessorModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<RabbitMqConsumerHost>()
                .AsImplementedInterfaces();
            builder.RegisterType<RabbitMqConnection>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqHealthCheck>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RabbitMqConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<RabbitMqQueueConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
