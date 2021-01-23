// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa {
    using Microsoft.IIoT.Protocols.OpcUa.Services;
    using Autofac;

    /// <summary>
    /// OPC UA client support
    /// </summary>
    public class ClientStack : Module {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<ClientServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DefaultSessionManager>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<SubscriptionServices>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<VariantEncoderFactory>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<StackLogger>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .AutoActivate();
            base.Load(builder);
        }
    }
}
