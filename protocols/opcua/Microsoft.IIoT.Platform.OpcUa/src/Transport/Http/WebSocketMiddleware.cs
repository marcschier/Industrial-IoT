// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa.Transport {
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Secure channel over websocket middleware
    /// </summary>
    public class WebSocketMiddleware : IMiddleware {

        /// <inheritdoc/>
        public Uri EndpointUrl { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="logger"></param>
        public WebSocketMiddleware(
            IWebSocketChannelListener listener, ILogger logger) {
            _listener = listener ??
                throw new ArgumentNullException(nameof(listener));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            EndpointUrl = null; // TODO
        }

        /// <summary>
        /// Handle websocket requests
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            if (context is null) {
                throw new ArgumentNullException(nameof(context));
            }
            if (next is null) {
                throw new ArgumentNullException(nameof(next));
            }
            if (context.WebSockets.IsWebSocketRequest) {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                if (webSocket != null) {
                    // pass to listener to decode secure channel binary stream
                    _listener.OnAccept(context, webSocket);
                    _logger.LogTrace("Accepted new websocket.");
                }
                else {
                    _logger.LogDebug("Accepted websocket was null.");
                }
                return;
            }
            await next(context).ConfigureAwait(false);
        }

        private readonly IWebSocketChannelListener _listener;
        private readonly ILogger _logger;
    }
}
