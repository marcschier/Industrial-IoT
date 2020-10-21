// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements node services as adapter through twin registry
    /// </summary>
    public sealed class NodeServicesAdapter : INodeServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="nodes"></param>
        public NodeServicesAdapter(ITwinRegistry registry,
            INodeServices<ConnectionModel> nodes) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            string twin, ValueReadRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeValueReadAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            string twin, ValueWriteRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeValueWriteAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            string twin, MethodMetadataRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeMethodGetMetadataAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            string twin, MethodCallRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeMethodCallAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            string twin, ReadRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeReadAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            string twin, WriteRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _nodes.NodeWriteAsync(conn, request, ct).ConfigureAwait(false);
        }

        private readonly ITwinRegistry _registry;
        private readonly INodeServices<ConnectionModel> _nodes;
    }
}
