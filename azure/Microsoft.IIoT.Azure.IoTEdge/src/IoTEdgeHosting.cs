// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {
    using Microsoft.IIoT.Azure.IoTEdge.Services;
    using Microsoft.IIoT.Azure.IoTEdge.Runtime;
    using Microsoft.IIoT.Azure.IoTEdge.Clients;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using Microsoft.IIoT.Extensions.Diagnostics.Services;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using Autofac;

    /// <summary>
    /// Injected iot hub edge hosting
    /// </summary>
    public sealed class IoTEdgeHosting : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Edge metrics collection and diagnostics
            builder.RegisterType<MetricsHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<EventSourceBroker>()
                .AsImplementedInterfaces().SingleInstance();

            builder.AddOptions();
            builder.RegisterType<IoTEdgeClientConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTEdgeMqttConfig>()
                .AsImplementedInterfaces();

            builder.RegisterType<IoTEdgeClientIdentity>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<EdgeletClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SasTokenGenerator>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<MemoryCache>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<IoTEdgeEventClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeMethodClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeMethodServer>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeMqttClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
