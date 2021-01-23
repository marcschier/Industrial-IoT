// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Services;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Storage;
    using Autofac;

    /// <summary>
    /// Injected twin control services
    /// </summary>
    public class TwinStorage : TwinEventBrokers {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Services
            builder.RegisterType<TwinRegistryServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRegistry));

            builder.RegisterType<TwinDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRepository))
                .IfNotRegistered(typeof(ITwinRegistry));

            base.Load(builder);
        }
    }
}
