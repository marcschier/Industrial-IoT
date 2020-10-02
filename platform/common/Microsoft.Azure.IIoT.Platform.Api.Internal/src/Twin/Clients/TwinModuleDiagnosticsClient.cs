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
    /// Client for supervisor diagnostics services on twin module
    /// </summary>
    public sealed class TwinModuleDiagnosticsClient : ISupervisorDiagnostics {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public TwinModuleDiagnosticsClient(IMethodClient client,
            IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<SupervisorStatusModel> GetSupervisorStatusAsync(
            string supervisorId, CancellationToken ct) {
            if (string.IsNullOrEmpty(supervisorId)) {
                throw new ArgumentNullException(nameof(supervisorId));
            }
            var sw = Stopwatch.StartNew();
            var result = await _client.CallMethodAsync(supervisorId, "GetStatus_V2",
                    null, null, ct).ConfigureAwait(false);
            _logger.Debug("Get twin supervisor {supervisorId} status took " +
                "{elapsed} ms.", supervisorId, sw.ElapsedMilliseconds);
            return _serializer.Deserialize<SupervisorStatusModel>(
                result);
        }

        /// <inheritdoc/>
        public async Task ResetSupervisorAsync(string supervisorId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(supervisorId)) {
                throw new ArgumentNullException(nameof(supervisorId));
            }
            var sw = Stopwatch.StartNew();
            _ = await _client.CallMethodAsync(supervisorId, "Reset_V2", null, null, ct).ConfigureAwait(false);
            _logger.Debug("Reset twin supervisor {supervisorId} took " +
                "{elapsed} ms.", supervisorId, sw.ElapsedMilliseconds);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
