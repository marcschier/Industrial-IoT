// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Api {
    using Microsoft.IIoT.Protocols.OpcUa.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Clients;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api;
    using Microsoft.IIoT.Extensions.Http.Clients;
    using Microsoft.IIoT.Extensions.Serializers;
    using Autofac;
    using System;

    /// <summary>
    /// Container builder extensions
    /// </summary>
    public static class ContainerBuilderEx {

        /// <summary>
        /// Add opc ua api
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        public static ContainerBuilder AddOpcUa(this ContainerBuilder builder,
            string endpointUrl = null) {
            return builder.AddOpcUa(options => options.OpcUaServiceUrl = endpointUrl);
        }

        /// <summary>
        /// Add opc ua api
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder AddOpcUa(this ContainerBuilder builder,
            Action<OpcUaApiOptions> configure) {

            builder.RegisterModule<HttpClientModule>();
            builder.RegisterModule<NewtonSoftJsonModule>();

            builder.RegisterType<OpcUaApiConfig>()
                .AsImplementedInterfaces();
            builder.Configure(configure);

            // Register twin, vault, and registry services clients
            builder.RegisterType<TwinServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<DiscoveryServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceClient>()
                .AsImplementedInterfaces();

            // ... with client event callbacks
            builder.RegisterType<DiscoveryServiceEvents>()
                .AsImplementedInterfaces();
            builder.RegisterType<TwinServiceEvents>()
                .AsImplementedInterfaces();
            builder.RegisterType<PublisherServiceEvents>()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}
