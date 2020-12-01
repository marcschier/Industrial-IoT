// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub {
    using Microsoft.Azure.IIoT.Azure.IoTHub.Clients;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Runtime;
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

            builder.RegisterType<IoTHubConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }

}
