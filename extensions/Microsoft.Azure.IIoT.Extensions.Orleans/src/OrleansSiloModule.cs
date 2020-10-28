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
    /// Injected orleans silo
    /// </summary>
    public class OrleansSiloModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<OrleansSiloHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansSiloHost));

            base.Load(builder);
        }
    }
}
