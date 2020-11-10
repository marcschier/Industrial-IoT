// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Kafka {
    using Microsoft.Azure.IIoT.Extensions.Kafka.Clients;
    using Microsoft.Azure.IIoT.Extensions.Kafka.Runtime;
    using Microsoft.Azure.IIoT.Extensions.Kafka.Services;
    using Microsoft.Azure.IIoT.Messaging.Handlers;
    using Autofac;

    /// <summary>
    /// Injected Mass transit event bus
    /// </summary>
    public class KafkaConsumerModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<KafkaConsumerHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<KafkaConsumerConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<KafkaServerConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<KafkaAdminClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DeviceEventHandler>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            base.Load(builder);
        }
    }
}