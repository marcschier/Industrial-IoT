// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka {
    using Microsoft.IIoT.Extensions.Kafka.Clients;
    using Microsoft.IIoT.Extensions.Kafka.Runtime;
    using Autofac;

    /// <summary>
    /// Injected Mass transit event bus
    /// </summary>
    public class KafkaProducerModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<KafkaAdminClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<KafkaProducerClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.AddOptions();
            builder.RegisterType<KafkaServerConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
