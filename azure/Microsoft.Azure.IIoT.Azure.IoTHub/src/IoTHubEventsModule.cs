// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub {
    using Microsoft.Azure.IIoT.Azure.IoTHub.Handlers;
    using Autofac;

    /// <summary>
    /// Injected iot hub service handlers
    /// </summary>
    public sealed class IoTHubEventsModule : IoTHubModule {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Services
            builder.RegisterType<IoTHubDeviceEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinChangeEventHandler>()
                .AsImplementedInterfaces();
            builder.RegisterType<DeviceLifecycleEventHandler>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
