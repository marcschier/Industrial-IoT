// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.Tunnel.Services {
    using Microsoft.Azure.IIoT.Http.Tunnel.Models;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Http.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.Azure.IIoT.Hub;

    /// <summary>
    /// Provides a http handler factory using method client as tunnel.
    /// This is for the cloud to module REST call tunnelling.
    /// Register on top of the HttpClientModule to use with injected
    /// <see cref="IHttpClient"/>.
    /// </summary>
    public sealed class HttpTunnelMethodClientFactory : IHttpHandlerFactory {

        /// <summary>
        /// Create handler factory
        /// </summary>
        /// <param name="client"></param>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public HttpTunnelMethodClientFactory(IMethodClient client, IJsonSerializer serializer,
            IEnumerable<IHttpHandler> handlers, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _handlers = handlers?.ToList() ?? new List<IHttpHandler>();
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public void Dispose() {
            // noop
        }

        /// <inheritdoc/>
        public TimeSpan Create(string name, out HttpMessageHandler handler) {
            var resource = name == HttpHandlerFactory.DefaultResourceId ? null : name;
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var del = new HttpHandlerDelegate(new HttpTunnelClientHandler(this),
#pragma warning restore IDE0067 // Dispose objects before losing scope
                resource, _handlers.Where(h => h.IsFor?.Invoke(resource) ?? true),
                null, _logger);
            handler = del;
            return del.MaxLifetime;
        }

        /// <summary>
        /// Http client handler for the tunnels
        /// </summary>
        private sealed class HttpTunnelClientHandler : Http.Default.HttpClientHandler {

            /// <inheritdoc/>
            public override bool SupportsAutomaticDecompression => true;

            /// <inheritdoc/>
            public override bool SupportsProxy => false;

            /// <inheritdoc/>
            public override bool SupportsRedirectConfiguration => false;

            /// <summary>
            /// Create handler
            /// </summary>
            /// <param name="outer"></param>
            public HttpTunnelClientHandler(HttpTunnelMethodClientFactory outer) {
                _outer = outer;
            }

            /// <inheritdoc/>
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken ct) {

                // Parse items from host name
                var deviceId = HubResource.Parse(
                    request.RequestUri.Host, out var hub, out var moduleId, true);
                var target = HubResource.Format(hub, deviceId, moduleId);

                // Create tunnel request
                var trequest = new HttpTunnelRequestModel {
                    ResourceId = null, // TODO - fill in somehow from outer handler
                    Uri = request.RequestUri.ToString(),
                    TraceId = "", // TODO
                    RequestHeaders = request.Headers?
                        .ToDictionary(h => h.Key, h => h.Value.ToList()),
                    Method = request.Method.ToString()
                };

                // Get content
                byte[] payload = null;
                if (request.Content != null) {
                    payload = await request.Content.ReadAsByteArrayAsync();
                    trequest.Body = payload;
                    trequest.ContentHeaders = request.Content.Headers?
                        .ToDictionary(h => h.Key, h => h.Value.ToList());
                }

                var input = _outer._serializer.SerializeToBytes(trequest).ToArray();
                var output = await _outer._client.CallMethodAsync(target,
                    "$tunnel", input, HttpTunnelRequestModel.SchemaName,
                    kDefaultTimeout, ct);
                var tResponse = _outer._serializer
                    .Deserialize<HttpTunnelResponseModel>(output);
                var response = new HttpResponseMessage((HttpStatusCode)tResponse.Status) {
                    ReasonPhrase = tResponse.Reason,
                    RequestMessage = request,
                    Content = tResponse.Payload == null ? null :
                        new ByteArrayContent(tResponse.Payload)
                };
                if (tResponse.Headers != null) {
                    foreach (var header in tResponse.Headers) {
                        response.Headers.TryAddWithoutValidation(
                            header.Key, header.Value);
                    }
                }
                return response;
            }

            private static readonly TimeSpan kDefaultTimeout = TimeSpan.FromMinutes(5);
            private readonly HttpTunnelMethodClientFactory _outer;
        }

        private readonly List<IHttpHandler> _handlers;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
    }
}
