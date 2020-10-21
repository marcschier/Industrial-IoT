// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Services;
    using Microsoft.Azure.IIoT.Platform.Publisher.Storage;
    using Autofac;

    /// <summary>
    /// Injected publisher storage
    /// </summary>
    public class PublisherStorage : PublisherEventBrokers {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Registry = control plane interface
            builder.RegisterType<WriterGroupRegistry>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<WriterGroupRegistrySync>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupStateUpdater))
                .IfNotRegistered(typeof(IDataSetWriterStateUpdater));

            // Underlying control plane configuration persistance 
            builder.RegisterType<WriterGroupDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupRepository));
            builder.RegisterType<DataSetWriterDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDataSetWriterRepository));
            builder.RegisterType<DataSetEntityDatabase>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDataSetEntityRepository));

            base.Load(builder);
        }
    }
}
