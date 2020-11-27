// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;

    /// <summary>
    /// Method client
    /// </summary>
    public sealed class IoTEdgeMethodClient : IJsonMethodClient {

        /// <inheritdoc/>
        public int MaxMethodPayloadSizeInBytes => 120 * 1024;

        /// <summary>
        /// Create method client
        /// </summary>
        /// <param name="client"></param>
        public IoTEdgeMethodClient(IIoTEdgeClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<string> CallMethodAsync(string target,
            string method, string payload, TimeSpan? timeout, CancellationToken ct) {
            var request = new MethodRequest(method, Encoding.UTF8.GetBytes(payload),
                timeout, null);
            MethodResponse response;
            var deviceId = HubResource.Parse(target, out _, out var moduleId);
            if (string.IsNullOrEmpty(moduleId)) {
                response = await _client.InvokeMethodAsync(deviceId, request, ct).ConfigureAwait(false);
            }
            else {
                response = await _client.InvokeMethodAsync(deviceId, moduleId, request, ct).ConfigureAwait(false);
            }
            if (response.Status != 200) {
                throw new MethodCallStatusException(
                    response.ResultAsJson, response.Status);
            }
            return response.ResultAsJson;
        }

        private readonly IIoTEdgeClient _client;
    }
}
