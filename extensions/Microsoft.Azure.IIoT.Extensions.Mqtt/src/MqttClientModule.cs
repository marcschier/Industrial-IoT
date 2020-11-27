// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Mqtt {
    using Microsoft.Azure.IIoT.Extensions.Mqtt.Clients;
    using Microsoft.Azure.IIoT.Extensions.Mqtt.Runtime;
    using Microsoft.Azure.IIoT.Messaging.Clients;
    using Microsoft.Azure.IIoT.Messaging;
    using Autofac;

    /// <summary>
    /// Injected mqtt client
    /// </summary>
    public class MqttClientModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<EventClientAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEventClient));
            builder.RegisterType<MqttClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IEventPublisherClient))
                .IfNotRegistered(typeof(IEventSubscriberClient));
            builder.RegisterType<MqttConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
