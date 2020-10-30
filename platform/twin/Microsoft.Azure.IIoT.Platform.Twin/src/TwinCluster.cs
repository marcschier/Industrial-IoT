// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Clients;
    using Microsoft.Azure.IIoT.Extensions.Orleans.Services;
    using Microsoft.Azure.IIoT.Extensions.Orleans;
    using Autofac;

    /// <summary>
    /// Injected twin services cluster if twin registry not yet registered
    /// </summary>
    public class TwinCluster : TwinServices {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<OrleansClientHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansClientHost))
                .IfNotRegistered(typeof(ITwinRegistry));
            builder.RegisterType<OrleansSiloHost>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IOrleansSiloHost))
                .IfNotRegistered(typeof(ITwinRegistry));

            base.Load(builder);

            // Grain clients
            builder.RegisterType<DataTransferGrainClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRegistry));
            builder.RegisterType<AddressSpaceGrainClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRegistry));
        }
    }
}
