// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor {
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor.Services;
    using Microsoft.Azure.IIoT.Messaging;
    using Autofac;

    /// <summary>
    /// Injected event processor host
    /// </summary>
    public class EventHubProcessorModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register event processor host for telemetry
            builder.RegisterType<EventProcessorHost>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(IEventProcessingHost));
            builder.RegisterType<EventProcessorFactory>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
