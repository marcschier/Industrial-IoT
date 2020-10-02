// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Client for Activation / placement of workergroup services in publisher workergroup supervisor
    /// </summary>
    public sealed class PublisherModuleActivationClient : IActivationServices<WriterGroupPlacementModel> {

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public PublisherModuleActivationClient(IMethodClient client, IJsonSerializer serializer,
            ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task ActivateAsync(WriterGroupPlacementModel placement,
            string secret, CancellationToken ct) {
            if (placement == null) {
                throw new ArgumentNullException(nameof(placement));
            }
            if (string.IsNullOrEmpty(placement.PublisherId)) {
                throw new ArgumentNullException(nameof(placement.PublisherId));
            }
            if (string.IsNullOrEmpty(placement.WriterGroupId)) {
                throw new ArgumentNullException(nameof(placement.WriterGroupId));
            }
            if (string.IsNullOrEmpty(secret)) {
                throw new ArgumentNullException(nameof(secret));
            }
            if (!secret.IsBase64()) {
                throw new ArgumentException("not base64", nameof(secret));
            }
            await CallServiceOnPublisherAsync("ActivateWriterGroup_V2", placement.PublisherId, new {
                placement.WriterGroupId,
                Secret = secret
            }, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeactivateAsync(WriterGroupPlacementModel placement,
            CancellationToken ct) {
            if (placement == null) {
                throw new ArgumentNullException(nameof(placement));
            }
            if (string.IsNullOrEmpty(placement.PublisherId)) {
                throw new ArgumentNullException(nameof(placement.PublisherId));
            }
            if (string.IsNullOrEmpty(placement.WriterGroupId)) {
                throw new ArgumentNullException(nameof(placement.WriterGroupId));
            }
            await CallServiceOnPublisherAsync("DeactivateWriterGroup_V2",
                placement.PublisherId, placement.WriterGroupId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper to invoke service
        /// </summary>
        /// <param name="service"></param>
        /// <param name="supervisorId"></param>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task CallServiceOnPublisherAsync(string service,
            string supervisorId, object payload, CancellationToken ct) {
            var sw = Stopwatch.StartNew();
            _ = await _client.CallMethodAsync(supervisorId, service,
                _serializer.SerializeToString(payload), null, ct).ConfigureAwait(false);
            _logger.Debug("Calling supervisor service '{service}' on " +
                "{supervisorId} took {elapsed} ms.", service, supervisorId,
                sw.ElapsedMilliseconds);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
