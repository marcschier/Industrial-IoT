// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka {
    using Microsoft.IIoT.Extensions.Kafka.Clients;
    using Microsoft.IIoT.Extensions.Kafka.Runtime;
    using Microsoft.IIoT.Extensions.Kafka.Services;
    using Microsoft.IIoT.Extensions.Messaging.Handlers;
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
            builder.RegisterType<KafkaServerConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<KafkaAdminClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DeviceEventHandler>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.AddOptions();
            builder.RegisterType<KafkaConsumerConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
