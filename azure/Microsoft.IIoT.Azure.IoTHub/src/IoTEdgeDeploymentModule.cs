// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub {
    using Microsoft.IIoT.Azure.IoTHub.Clients;
    using Microsoft.IIoT.Azure.IoTHub.Runtime;
    using Microsoft.IIoT.Azure.IoTHub.Deploy;
    using Autofac;

    /// <summary>
    /// Injected edge deployment services
    /// </summary>
    public sealed class IoTEdgeDeploymentModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<IoTHubConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTHubConfigurationClient>()
                .AsImplementedInterfaces();

            builder.RegisterType<ContainerRegistryConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTEdgeBaseDeployment>()
                .AsSelf()
                .AsImplementedInterfaces().SingleInstance();

            base.Load(builder);
        }
    }
}
