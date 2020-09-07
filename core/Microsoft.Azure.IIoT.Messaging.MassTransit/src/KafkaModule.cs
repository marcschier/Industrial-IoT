// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit {
    using Microsoft.Azure.IIoT.Messaging.MassTransit.Services;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Tasks;
    using Autofac;
    using global::MassTransit;
    using global::MassTransit.AutofacIntegration;

    /// <summary>
    /// Injected Mass transit event bus
    /// </summary>
    public abstract class KafkaModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<MassTransitEventBusHost>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MassTransitEventBus>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));
            builder.AddMassTransit(x => Configure(x));
            base.Load(builder);
        }

        /// <summary>
        /// Configure mass transit
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        protected abstract void Configure(
            IContainerBuilderBusConfigurator configure);
    }
}
