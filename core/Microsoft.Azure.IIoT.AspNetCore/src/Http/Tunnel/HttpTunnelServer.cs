// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Http.Tunnel {
    using Microsoft.Azure.IIoT.AspNetCore;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Hosting.Services;
    using Microsoft.Azure.IIoT.Http.Tunnel.Models;
    using Microsoft.Azure.IIoT.Rpc.Default;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles http requests through chunk server and passes them
    /// to the application server instance.  The tunnel can handle
    /// straight requests to a path identified by the method string
    /// or the tunnel model which provides a pure http request
    /// response pattern that includes content and request headers.
    /// </summary>
    /// <remarks>
    /// Handles cloud initiated http requests e.g. at the edge.
    /// These can be tunnelled through e.g. device methods.
    /// Not to be confused with the cloud side HttpTunnelServer
    /// that unpacks http tunnel requests from edge.
    /// </remarks>
    public sealed class HttpTunnelServer : IAppServer, IMethodRouter {

        /// <inheritdoc/>
        public HttpTunnelServer(IJsonSerializer serializer, ILogger logger) {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public void Start(IServiceProvider services, RequestDelegate request) {
            // Start tunnel
            if (_tunnel != null) {
                throw new ResourceInvalidStateException("Tunnel already started.");
            }
            _tunnel = new HttpTunnel(services, request, _serializer, _logger);
        }

        /// <inheritdoc/>
        public async Task<byte[]> InvokeAsync(string target,
            string method, byte[] payload, string contentType) {
            if (_tunnel == null) {
                throw new MethodCallException("Tunnel not yet started.");
            }
            return await _tunnel.InvokeAsync(target, method, payload,
                contentType).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose() {
            // Stop tunnel
            _tunnel?.Dispose();
            _tunnel = null;
        }

        /// <summary>
        /// An immutable tunnel instance
        /// </summary>
        internal class HttpTunnel : IMethodHandler, IDisposable {

            /// <summary>
            /// Create tunnel
            /// </summary>
            /// <param name="services"></param>
            /// <param name="request"></param>
            /// <param name="serializer"></param>
            /// <param name="logger"></param>
            public HttpTunnel(IServiceProvider services, RequestDelegate request,
                IJsonSerializer serializer, ILogger logger) {
                _serializer = serializer;
                _delegate = request;
                _services = services;
                _chunks = new ChunkMethodServer(_serializer, logger);
            }

            /// <inheritdoc/>
            public async Task<byte[]> InvokeAsync(string target, string method,
                byte[] payload, string contentType) {
                if (method == _chunks.MethodName) {
                    // Pass to chunk server
                    return await _chunks.InvokeAsync(target, payload, contentType, this).ConfigureAwait(false);
                }

                var isSimpleCall = contentType != HttpTunnelRequestModel.SchemaName;
                var request = new HttpTunnelRequest {
                    Protocol = "TUNNEL"
                };
                if (!isSimpleCall) {
                    if (payload == null) {
                        throw new ArgumentNullException(nameof(payload));
                    }
                    // Deserialize http tunnel payload
                    var inbound = _serializer.Deserialize<HttpTunnelRequestModel>(payload);
                    request.Method = inbound.Method ?? "GET";
                    request.Payload = inbound.Body ?? kEmptyPayload;
                    request.RawTarget = inbound.Uri;

                    if (inbound.ContentHeaders != null) {
                        foreach (var item in inbound.ContentHeaders) {
                            request.Headers.TryAdd(item.Key,
                                new StringValues(item.Value.ToArray()));
                        }
                    }
                    if (inbound.RequestHeaders != null) {
                        foreach (var item in inbound.RequestHeaders) {
                            request.Headers.TryAdd(item.Key,
                                new StringValues(item.Value.ToArray()));
                        }
                    }

                    var uri = new Uri(inbound.Uri);
                    request.Scheme = uri.Scheme;
                    request.Path = uri.AbsolutePath;
                    request.QueryString = uri.Query;
                    request.TraceIdentifier = inbound.TraceId;
                }
                else {
                    if (contentType != null && contentType != ContentMimeType.Json) {
                        throw new ArgumentException(
                            $"{contentType} must be null or {ContentMimeType.Json}",
                                nameof(contentType));
                    }

                    if (string.IsNullOrEmpty(method)) {
                        method = "/";
                    }
                    else if (method[0] != '/') {
                        method = "/" + method;
                    }

                    // Raw json payload call (device method)
                    request.Payload = payload ?? kEmptyPayload;
                    request.Method = "POST";
                    request.Path = method; // TODO: Route correctly
                    request.RawTarget = method;

                    request.Headers.TryAdd(HeaderNames.ContentType,
                        ContentMimeType.Json);
                    request.Headers.TryAdd(HeaderNames.ContentEncoding,
                        ContentMimeType.Json);
                }

                // Create context
                var factory = _services.GetService<IHttpContextFactory>();
                using var buffer = new MemoryStream();
                var response = new HttpTunnelResponse(buffer);
                var features = new FeatureCollection();
                features.Set<IHttpRequestFeature>(request);
                features.Set<IHttpRequestIdentifierFeature>(request);
                features.Set<IHttpResponseFeature>(response);
                features.Set<IHttpResponseBodyFeature>(response);
                features.Set<IHttpBodyControlFeature>(response);
                var context = factory.Create(features);

                // Handle
                await _delegate(context).ConfigureAwait(false);

                if (!isSimpleCall) {
                    // Serialize http back
                    var outbound = new HttpTunnelResponseModel {
                        Payload = response.Payload,
                        RequestId = request.TraceIdentifier,
                        Status = response.StatusCode,
                        Reason = response.ReasonPhrase,
                        Headers = response.Headers?
                            .ToDictionary(k => k.Key, v => v.Value.ToList()),
                    };
                    return _serializer.SerializeToBytes(outbound).ToArray();
                }
                if (response.StatusCode != (int)HttpStatusCode.OK) {
                    throw new MethodCallStatusException(
                        response.StatusCode, response.ReasonPhrase);
                }
                return response.Payload;
            }

            /// <inheritdoc/>
            public void Dispose() {
                _chunks.Dispose();
            }

            private static readonly byte[] kEmptyPayload = Array.Empty<byte>();
            private readonly IJsonSerializer _serializer;
            private readonly ChunkMethodServer _chunks;
            private readonly IServiceProvider _services;
            private readonly RequestDelegate _delegate;
        }

        /// <summary>
        /// Request
        /// </summary>
        private class HttpTunnelRequest : IHttpRequestFeature,
            IHttpRequestIdentifierFeature {

            /// <inheritdoc/>
            public Stream Body { get; set; }

            /// <summary>
            /// Payload
            /// </summary>
            internal byte[] Payload {
                get => (Body as MemoryStream).ToArray();
                set => Body = new MemoryStream(value);
            }

            /// <inheritdoc/>
            public IHeaderDictionary Headers { get; set; }
                = new HeaderDictionary();

            /// <inheritdoc/>
            public string Method { get; set; }
            /// <inheritdoc/>
            public string Path { get; set; }
            /// <inheritdoc/>
            public string PathBase { get; set; } = string.Empty;
            /// <inheritdoc/>
            public string Protocol { get; set; } = string.Empty;
            /// <inheritdoc/>
            public string QueryString { get; set; }
            /// <inheritdoc/>
            public string RawTarget { get; set; }
            /// <inheritdoc/>
            public string Scheme { get; set; }
            /// <inheritdoc/>
            public string TraceIdentifier { get; set; }
        }

        /// <summary>
        /// Response
        /// </summary>
        private class HttpTunnelResponse : StreamResponseBodyFeature,
            IHttpResponseFeature, IHttpResponseBodyFeature,
            IHttpBodyControlFeature {

            /// <inheritdoc/>
            public HttpTunnelResponse(Stream stream)
                : base(stream) {
            }

            /// <inheritdoc/>
            public Stream Body {
                get => Stream;
                set => throw new NotSupportedException();
            }

            internal byte[] Payload =>
                (Stream as MemoryStream)?.ToArray();

            /// <inheritdoc/>
            public bool HasStarted =>
                Body.Position != 0;
            /// <inheritdoc/>
            public IHeaderDictionary Headers { get; set; } =
                new HeaderDictionary();
            /// <inheritdoc/>
            public string ReasonPhrase { get; set; } =
                string.Empty;
            /// <inheritdoc/>
            public int StatusCode { get; set; } =
                (int)HttpStatusCode.OK;

            /// <inheritdoc/>
            public void OnCompleted(Func<object, Task> callback,
                object state) {
            }

            /// <inheritdoc/>
            public void OnStarting(Func<object, Task> callback,
                object state) {
            }

            /// <inheritdoc/>
            public bool AllowSynchronousIO { get; set; } = false;
        }

        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private HttpTunnel _tunnel;
    }
}