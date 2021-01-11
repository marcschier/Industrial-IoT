// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub {
    using Microsoft.IIoT.Azure.IoTHub.Clients;
    using Microsoft.IIoT.Azure.IoTHub.Runtime;
    using Microsoft.IIoT.Azure.IoTHub.Services;
    using Autofac;

    /// <summary>
    /// Injected edge deployment services
    /// </summary>
    public sealed class IoTEdgeDeploymentModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<IoTHubServiceConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTHubConfigurationClient>()
                .AsImplementedInterfaces();

            builder.AddOptions();
            builder.RegisterType<ContainerRegistryConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTEdgeBaseDeployment>()
                .AsSelf()
                .AsImplementedInterfaces().SingleInstance();

            base.Load(builder);
        }
    }
}
