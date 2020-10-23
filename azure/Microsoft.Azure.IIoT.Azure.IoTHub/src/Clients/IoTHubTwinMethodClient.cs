// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Clients {
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Method client using twin services
    /// </summary>
    public sealed class IoTHubTwinMethodClient : IJsonMethodClient {

        /// <inheritdoc/>
        public int MaxMethodPayloadSizeInBytes => 120 * 1024;

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="logger"></param>
        public IoTHubTwinMethodClient(IDeviceTwinServices twin, ILogger logger) {
            _twin = twin ?? throw new ArgumentNullException(nameof(twin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<string> CallMethodAsync(string target,
            string method, string payload, TimeSpan? timeout, CancellationToken ct) {
            var deviceId = HubResource.Parse(target, out _, out var moduleId);
            _logger.LogTrace("Call {method} on {device} ({module}) with {payload}... ",
                method, deviceId, moduleId, payload);
            var result = await _twin.CallMethodAsync(deviceId, moduleId,
                new MethodParameterModel {
                    Name = method,
                    ResponseTimeout = timeout ?? TimeSpan.FromSeconds(kDefaultMethodTimeout),
                    JsonPayload = payload
                }, ct).ConfigureAwait(false);
            if (result.Status != 200) {
                _logger.LogDebug("Call {method} on {device} ({module}) with {payload} " +
                    "returned with error {status}: {result}",
                    method, deviceId, moduleId, payload, result.Status, result.JsonPayload);
                throw new MethodCallStatusException(result.JsonPayload, result.Status);
            }
            return result.JsonPayload;
        }

        private readonly IDeviceTwinServices _twin;
        private readonly ILogger _logger;
        private const int kDefaultMethodTimeout = 300; // 5 minutes - default is 30 seconds
    }
}
