// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka {
    using Microsoft.Azure.IIoT.Messaging.Kafka.Services;
    using Microsoft.Azure.IIoT.Messaging.Kafka.Handlers;
    using Autofac;

    /// <summary>
    /// Injected Mass transit event bus
    /// </summary>
    public abstract class KafkaConsumerModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<KafkaConsumerHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<KafkaDeviceEventHandler>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            base.Load(builder);
        }
    }
}
