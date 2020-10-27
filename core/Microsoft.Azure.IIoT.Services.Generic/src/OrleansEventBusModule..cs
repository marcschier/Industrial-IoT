// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans {
    using Microsoft.Azure.IIoT.Services.Orleans.Clients;
    using Microsoft.Azure.IIoT.Services.Orleans.Grains;
    using Microsoft.Azure.IIoT.Messaging.Default;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Tasks;
    using Autofac;

    /// <summary>
    /// Injected orleans event bus
    /// </summary>
    public class OrleansEventBusModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<OrleansEventBusClient>()
                .AsImplementedInterfaces().InstancePerDependency();

            builder.RegisterType<OrleansSiloHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansSiloHost));
            builder.RegisterType<OrleansClientHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansClientHost));

            builder.RegisterType<EventBusHost>().AsSelf()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<TaskProcessor>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskProcessor));
            base.Load(builder);
        }
    }
}
