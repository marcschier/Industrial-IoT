// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub {
    using Microsoft.IIoT.Azure.EventHub.Clients;
    using Microsoft.IIoT.Azure.EventHub.Runtime;
    using Autofac;

    /// <summary>
    /// Injected event hub client
    /// </summary>
    public class EventHubClientModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register event queue client
            builder.RegisterType<EventHubQueueClient>()
                .AsImplementedInterfaces();

            builder.AddOptions();
            builder.RegisterType<EventHubClientConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
