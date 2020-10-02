// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Clients {
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
    /// Client for publisher diagnostics services on publisher module
    /// </summary>
    public sealed class PublisherModuleDiagnosticsClient : IPublisherDiagnostics {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public PublisherModuleDiagnosticsClient(IMethodClient client,
            IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<SupervisorStatusModel> GetPublisherStatusAsync(
            string publisherId, CancellationToken ct) {
            if (string.IsNullOrEmpty(publisherId)) {
                throw new ArgumentNullException(nameof(publisherId));
            }
            var sw = Stopwatch.StartNew();
            var result = await _client.CallMethodAsync(publisherId,
                "GetStatus_V2", null, null, ct).ConfigureAwait(false);
            _logger.Debug("Get publisher supervisor {publisherId} status " +
                "took {elapsed} ms.", publisherId, sw.ElapsedMilliseconds);
            return _serializer.Deserialize<SupervisorStatusModel>(
                result);
        }

        /// <inheritdoc/>
        public async Task ResetPublisherAsync(string publisherId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(publisherId)) {
                throw new ArgumentNullException(nameof(publisherId));
            }
            var sw = Stopwatch.StartNew();
            _ = await _client.CallMethodAsync(publisherId,
                "Reset_V2", null, null, ct).ConfigureAwait(false);
            _logger.Debug("Reset publisher supervisor {publisherId} took " +
                "{elapsed} ms.", publisherId, sw.ElapsedMilliseconds);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
