// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq {
    using Microsoft.Azure.IIoT.Services.RabbitMq.Clients;
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
                .AsImplementedInterfaces();
            builder.RegisterType<RabbitMqConnection>()
                .AsImplementedInterfaces().SingleInstance();
            base.Load(builder);
        }
    }
}
