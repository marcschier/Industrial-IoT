// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Default;
    using Autofac;

    /// <summary>
    /// Injected publisher event brokers
    /// </summary>
    public class PublisherEventBrokers : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<DataSetWriterEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<WriterGroupEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<PublishedDataSetEventBroker>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
