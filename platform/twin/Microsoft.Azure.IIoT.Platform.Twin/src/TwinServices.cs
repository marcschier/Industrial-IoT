// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Services;
    using Microsoft.Azure.IIoT.Platform.Twin.Clients;
    using Autofac;

    /// <summary>
    /// Injected twin services
    /// </summary>
    public sealed class TwinServices : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Core services of the twin
            builder.RegisterType<DataTransferServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AddressSpaceServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
 
            // Adapted to endpoint id with endpoint registry as dependency
            builder.RegisterType<TransferServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<NodeServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<HistoricAccessServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<HistorianServicesAdapter<string>>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<BrowseServicesAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
