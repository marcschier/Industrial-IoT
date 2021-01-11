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
    /// Injected iot hub service client
    /// </summary>
    public class IoTHubSupportModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            // Clients
            builder.RegisterType<IoTHubServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTHubConfigurationClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTHubSasTokenValidator>()
                .AsImplementedInterfaces();
            builder.RegisterType<IoTHubTwinMethodClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<ProvisioningServiceClient>()
                .AsImplementedInterfaces();

            builder.AddOptions();
            builder.RegisterType<IoTHubServiceConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<ProvisioningServiceConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }

}
