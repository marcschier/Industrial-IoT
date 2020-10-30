// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Services;
    using Microsoft.Azure.IIoT.Platform.Twin.Clients;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Discovery;
    using Autofac;

    /// <summary>
    /// Injected twin services
    /// </summary>
    public class TwinServices : TwinStorage {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Core services of the twin
            builder.RegisterType<DataTransferServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITransferServices<ConnectionModel>));
            builder.RegisterType<AddressSpaceServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(INodeServices<ConnectionModel>))
                .IfNotRegistered(typeof(IBrowseServices<ConnectionModel>))
                .IfNotRegistered(typeof(IHistoricAccessServices<ConnectionModel>));

            // Adapted to twin with twin registry as dependency
            builder.RegisterType<DataTransferAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ITransferServices<string>));
            builder.RegisterType<AddressSpaceAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(INodeServices<string>))
                .IfNotRegistered(typeof(IBrowseServices<string>))
                .IfNotRegistered(typeof(IHistoricAccessServices<string>));
            builder.RegisterType<HistorianServicesAdapter<ConnectionModel>>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IHistorianServices<ConnectionModel>));
            builder.RegisterType<HistorianServicesAdapter<string>>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IHistorianServices<string>));
            builder.RegisterType<CertificateServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ICertificateServices<string>));

            base.Load(builder);
        }
    }
}
