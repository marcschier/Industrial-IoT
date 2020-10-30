// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Mock {
    using Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Azure.IIoT.Tasks.Services;
    using Microsoft.Azure.IIoT.Serializers;
    using Autofac;

    /// <summary>
    /// Injected mock edge framework module
    /// </summary>
    public sealed class IoTHubMockModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Module and device client simulation
            builder.RegisterType<IoTHubClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<IoTEdgeEventClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeMethodClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<IoTEdgeMethodServer>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            // If not already registered, register a task scheduler
            builder.RegisterType<DefaultScheduler>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskScheduler));

            // Register default serializers...
            builder.RegisterModule<NewtonSoftJsonModule>();
            base.Load(builder);
        }
    }
}
