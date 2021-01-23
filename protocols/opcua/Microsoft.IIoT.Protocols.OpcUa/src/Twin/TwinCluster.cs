// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Clients;
    using Microsoft.IIoT.Extensions.Orleans.Services;
    using Microsoft.IIoT.Extensions.Orleans;
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

            // Grain clients
            builder.RegisterType<DataTransferGrainClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRegistry));
            builder.RegisterType<AddressSpaceGrainClient>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITwinRegistry));

            base.Load(builder);
        }
    }
}
