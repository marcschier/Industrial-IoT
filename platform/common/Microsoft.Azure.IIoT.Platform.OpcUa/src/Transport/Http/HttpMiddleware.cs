// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.OpcUa.Transport {
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;
    using Opc.Ua;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Decodes and forwards http based requests to the server and returns
    /// server responses to clients.
    /// </summary>
    public class HttpMiddleware : IMiddleware {

        /// <summary>
        /// Creates middleware to forward requests to controller
        /// </summary>
        /// <param name="encoder"></param>
        /// <param name="listener"></param>
        /// <param name="logger"></param>
        public HttpMiddleware(IMessageSerializer encoder,
            IHttpChannelListener listener, ILogger logger) {
            _encoder = encoder ??
                throw new ArgumentNullException(nameof(encoder));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _listener = listener ??
                throw new ArgumentNullException(nameof(listener));
        }

        /// <summary>
        /// Middleware invoke entry point which forwards to controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            var handled = await ProcessAsync(context).ConfigureAwait(false);
            if (!handled) {
                await next(context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Middleware invoke entry point which forwards to controller
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ProcessAsync(HttpContext context) {
            if (!context.Request.Method.Equals(HttpMethods.Post)) {
                return false;
            }
            // Decode request
            var message = _encoder.Decode(context.Request.ContentType,
                context.Request.Body);
            if (message is not IServiceRequest request) {
                _logger.LogDebug("Bad UA service request.");
                return false;
            }
            try {
                _logger.LogTrace("Processing UA request...");
                var response = await _listener.ProcessAsync(context, request).ConfigureAwait(false);
                // Encode content as per encoding requested
                context.Response.ContentType = context.Request.ContentType;
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                // context.Response.ContentLength = buffer.Length;
                using (context.Response.Body) {
                    _encoder.Encode(context.Request.ContentType,
                        context.Response.Body, response);
                }
                _logger.LogTrace("Processed UA request.");
            }
            catch {
                context.Response.ContentType = context.Request.ContentType;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            return true;
        }

        private readonly ILogger _logger;
        private readonly IHttpChannelListener _listener;
        private readonly IMessageSerializer _encoder;
    }
}
