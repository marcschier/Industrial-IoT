// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Services;
    using Microsoft.Azure.IIoT.Platform.Twin.Storage;
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
            builder.RegisterType<TwinRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<TwinDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRepository));

            base.Load(builder);
        }
    }
}
