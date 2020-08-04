// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge {
    using Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting;
    using Microsoft.Azure.IIoT.Azure.IoTEdge.Clients;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Diagnostics.Default;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Autofac;

    /// <summary>
    /// Injected iot hub edge hosting context
    /// </summary>
    public sealed class IoTEdgeHosted : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Edge metrics collection or server hosting
            builder.RegisterType<MetricsHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // Register sdk, edgelet client and token generators
            builder.RegisterType<IoTSdkFactory>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<EventSourceBroker>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EdgeletClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SasTokenGenerator>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<MemoryCache>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // .... and module host
            builder.RegisterType<IoTEdgeModuleHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeHostManager>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
