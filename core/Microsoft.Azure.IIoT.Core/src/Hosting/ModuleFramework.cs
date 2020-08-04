// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting {
    using Microsoft.Azure.IIoT.Hosting.Services;
    using Microsoft.Azure.IIoT.Http.Tunnel;
    using Microsoft.Azure.IIoT.Tasks.Default;
    using Microsoft.Azure.IIoT.Tasks;
    using Autofac;

    /// <summary>
    /// Injected module framework module
    /// </summary>
    public sealed class ModuleFramework : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            // Auto wire property for circular dependency resolution
            builder.RegisterType<MethodRouter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .PropertiesAutowired(
                    PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<SettingsRouter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .PropertiesAutowired(
                    PropertyWiringOptions.AllowCircularDependencies);

            // If not already registered, register task scheduler
#if USE_DEFAULT_FACTORY
            builder.RegisterType<DefaultScheduler>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskScheduler));
#else
            builder.RegisterType<LimitingScheduler>()
                .AsImplementedInterfaces().SingleInstance()
                .IfNotRegistered(typeof(ITaskScheduler));
#endif
            // Register http (tunnel) client module
            builder.RegisterModule<HttpTunnelClient>();

            base.Load(builder);
        }
    }
}
