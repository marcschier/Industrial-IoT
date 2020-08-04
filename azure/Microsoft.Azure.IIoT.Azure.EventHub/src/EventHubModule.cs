// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub {
    using Microsoft.Azure.IIoT.Azure.EventHub.Clients;
    using Autofac;

    /// <summary>
    /// Injected event hub client
    /// </summary>
    public class EventHubModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register event processor host for telemetry
            builder.RegisterType<EventHubNamespaceClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
