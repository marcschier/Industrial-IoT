// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Services;
    using Microsoft.IIoT.Platform.Publisher.Clients;
    using Autofac;

    /// <summary>
    /// Injected publisher data plane and control plane services
    /// </summary>
    public sealed class PublisherServices : PublisherStorage {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            base.Load(builder);

            // Data plane and data plane connectivity to control plane
            builder.RegisterType<DataSetWriterSubscription>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDataSetWriterDataSource));
            builder.RegisterType<DataSetWriterDiagnostics>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDataSetWriterDiagnostics));
            builder.RegisterType<SimpleWriterGroupManager>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupDataSource));
            builder.RegisterType<SimpleWriterGroupDataSource>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupDataSource));
            builder.RegisterType<SimpleNetworkMessageSink>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupDataSink));

            // Data plane signalling
            builder.RegisterType<WriterGroupStateAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IWriterGroupStateReporter));
            builder.RegisterType<DataSetWriterStateAdapter>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(IDataSetWriterStateReporter));
        }
    }
}
