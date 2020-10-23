// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Method server implementation
    /// </summary>
    public sealed class IoTEdgeMethodServer : IHostProcess {

        /// <summary>
        /// Create module host
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        /// <param name="identity"></param>
        /// <param name="routers"></param>
        public IoTEdgeMethodServer(IIoTEdgeClient client, ILogger logger,
            IIdentity identity, IEnumerable<IMethodHandler> routers) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _routers = routers?.ToList() ??
                throw new ArgumentNullException(nameof(routers));
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_started) {
                    await _client.SetMethodDefaultHandlerAsync(null, null).ConfigureAwait(false);
                    _started = false;
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_started) {
                    await _client.SetMethodDefaultHandlerAsync((request, _) =>
                        InvokeMethodAsync(request), null).ConfigureAwait(false);
                    _started = true;
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
            _lock.Dispose();
        }

        /// <summary>
        /// Invoke method handler on method router
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<MethodResponse> InvokeMethodAsync(MethodRequest request) {
            const int kMaxMessageSize = 127 * 1024;
            foreach (var router in _routers) {
                try {
                    var result = await router.InvokeAsync(_identity.AsHubResource(),
                        request.Name, request.Data, ContentMimeType.Json).ConfigureAwait(false);
                    if (result.Length > kMaxMessageSize) {
                        _logger.Error("Result (Payload too large => {Length}", result.Length);
                        return new MethodResponse((int)HttpStatusCode.RequestEntityTooLarge);
                    }
                    return new MethodResponse(result, 200);
                }
                catch (NotSupportedException) {
                    continue;
                }
                catch (MethodCallStatusException mex) {
                    var payload = Encoding.UTF8.GetBytes(mex.ResponsePayload);
                    return new MethodResponse(payload.Length > kMaxMessageSize ? null : payload,
                        mex.Result);
                }
                catch (Exception) {
                    return new MethodResponse((int)HttpStatusCode.InternalServerError);
                }
            }
            return new MethodResponse((int)HttpStatusCode.InternalServerError);
        }


        private readonly IIoTEdgeClient _client;
        private readonly List<IMethodHandler> _routers;
        private readonly ILogger _logger;
        private readonly IIdentity _identity;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private bool _started;
    }
}
