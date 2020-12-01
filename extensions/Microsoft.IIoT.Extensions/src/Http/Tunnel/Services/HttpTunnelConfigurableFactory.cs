// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Http.Tunnel.Services {
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Http;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Collections.Generic;

    /// <summary>
    /// Makes the tunnel configurable
    /// </summary>
    public sealed class HttpTunnelConfigurableFactory : IHttpHandlerFactory {

        /// <inheritdoc/>
        public HttpTunnelConfigurableFactory(IEventClient client,
            IJsonSerializer serializer, IOptionsMonitor<HttpTunnelOptions> options,
            IEnumerable<IHttpHandler> handlers, IIdentity identity, ILogger logger) {
            _tunnel = new HttpTunnelEventClientFactory(client, serializer,
                handlers, identity, logger);
            _fallback = new HttpHandlerFactory(handlers, logger);
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public HttpTunnelConfigurableFactory(IEventClient client, IWebProxy proxy,
            IJsonSerializer serializer, IOptionsMonitor<HttpTunnelOptions> options,
            IEnumerable<IHttpHandler> handlers, IIdentity identity, ILogger logger) {
            _tunnel = new HttpTunnelEventClientFactory(client, serializer,
                handlers, identity, logger);
            _fallback = new HttpHandlerFactory(handlers, proxy, logger);
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public TimeSpan Create(string resource, out HttpMessageHandler handler) {
            return _options.CurrentValue.UseTunnel &&
                (resource == null || !resource.StartsWith(Resource.Local)) ?
                _tunnel.Create(resource, out handler) :
                _fallback.Create(resource, out handler);
        }

        private readonly HttpTunnelEventClientFactory _tunnel;
        private readonly HttpHandlerFactory _fallback;
        private readonly IOptionsMonitor<HttpTunnelOptions> _options;
    }
}
