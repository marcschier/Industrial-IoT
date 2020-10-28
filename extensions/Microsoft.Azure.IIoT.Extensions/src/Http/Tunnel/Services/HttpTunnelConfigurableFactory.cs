// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.Tunnel.Services {
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Http.Clients;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Collections.Generic;

    /// <summary>
    /// Makes the tunnel configurable
    /// </summary>
    public sealed class HttpTunnelConfigurableFactory : IHttpHandlerFactory, IHttpTunnelConfig {

        /// <inheritdoc/>
        public bool UseTunnel { get; set; }

        /// <inheritdoc/>
        public HttpTunnelConfigurableFactory(IEventClient client,
            IJsonSerializer serializer, IEnumerable<IHttpHandler> handlers,
            IIdentity identity, ILogger logger) {
            _tunnel = new HttpTunnelEventClientFactory(client, serializer, handlers, identity, logger);
            _fallback = new HttpHandlerFactory(handlers, logger);
        }

        /// <inheritdoc/>
        public HttpTunnelConfigurableFactory(IEventClient client, IWebProxy proxy,
            IJsonSerializer serializer, IEnumerable<IHttpHandler> handlers,
            IIdentity identity, ILogger logger) {
            _tunnel = new HttpTunnelEventClientFactory(client, serializer, handlers, identity, logger);
            _fallback = new HttpHandlerFactory(handlers, proxy, logger);
        }

        /// <inheritdoc/>
        public TimeSpan Create(string resource, out HttpMessageHandler handler) {
            return UseTunnel && (resource == null || !resource.StartsWith(Resource.Local)) ?
                _tunnel.Create(resource, out handler) :
                _fallback.Create(resource, out handler);
        }

        private readonly HttpTunnelEventClientFactory _tunnel;
        private readonly HttpHandlerFactory _fallback;
    }
}
