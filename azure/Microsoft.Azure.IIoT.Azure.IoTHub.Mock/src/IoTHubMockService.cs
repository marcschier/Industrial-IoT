// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Mock {
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Rpc.Default;
    using Autofac;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Handlers;

    /// <summary>
    /// Injected mock framework module
    /// </summary>
    public sealed class IoTHubMockService : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // IoT hub and storage simulation
            builder.RegisterType<IoTHubServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Adapters
            builder.RegisterType<IoTHubDeviceEventHandler>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTHubTwinMethodClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<ChunkMethodClient>()
                .AsImplementedInterfaces();

            // Register default serializers...
            builder.RegisterModule<NewtonSoftJsonModule>();

            base.Load(builder);
        }
    }
}
