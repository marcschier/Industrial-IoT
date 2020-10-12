// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Client for Activation services in supervisor
    /// </summary>
    public sealed class TwinModuleActivationClient : IActivationServices<EndpointInfoModel> {

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public TwinModuleActivationClient(IMethodClient client, IJsonSerializer serializer,
            ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task ActivateAsync(EndpointInfoModel registration,
            string secret, CancellationToken ct) {
            if (registration == null) {
                throw new ArgumentNullException(nameof(registration));
            }
            if (string.IsNullOrEmpty(registration.Id)) {
                throw new ArgumentException("Missing registration id", nameof(registration));
            }
            if (string.IsNullOrEmpty(secret)) {
                throw new ArgumentNullException(nameof(secret));
            }
            if (!secret.IsBase64()) {
                throw new ArgumentException("not base64", nameof(secret));
            }
            await CallServiceAsync("ActivateEndpoint_V2", null/* TODO registration.SupervisorId */, new {
                registration.Id,
                Secret = secret
            }, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeactivateAsync(EndpointInfoModel registration,
            CancellationToken ct) {
            if (registration == null) {
                throw new ArgumentNullException(nameof(registration));
            }
            if (string.IsNullOrEmpty(registration.Id)) {
                throw new ArgumentException("Missing registration id", nameof(registration));
            }
            await CallServiceAsync("DeactivateEndpoint_V2", null/* TODO registration.SupervisorId */,
                registration.Id, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper to invoke service
        /// </summary>
        /// <param name="service"></param>
        /// <param name="target"></param>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task CallServiceAsync(string service,
            string target, object payload, CancellationToken ct) {
            var sw = Stopwatch.StartNew();
            _ = await _client.CallMethodAsync(target, service,
                _serializer.SerializeToString(payload), null, ct).ConfigureAwait(false);
            _logger.Debug("Calling supervisor service '{service}' on " +
                "{target} took {elapsed} ms.", service, target,
                    sw.ElapsedMilliseconds);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
