// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements node services as adapter through twin registry
    /// </summary>
    public sealed class AddressSpaceAdapter :
        IBrowseServices<string>, IHistoricAccessServices<string>, INodeServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="browse"></param>
        /// <param name="nodes"></param>
        /// <param name="history"></param>
        public AddressSpaceAdapter(ITwinRegistry registry,
            IBrowseServices<ConnectionModel> browse, INodeServices<ConnectionModel> nodes,
            IHistoricAccessServices<ConnectionModel> history) {

            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            _browse = browse ?? throw new ArgumentNullException(nameof(browse));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            string twin, BrowseRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _browse.NodeBrowseFirstAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            string twin, BrowseNextRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _browse.NodeBrowseNextAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            string twin, BrowsePathRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _browse.NodeBrowsePathAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            string twin, HistoryReadRequestModel<VariantValue> request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryReadAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryReadNextAsync(conn, request, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            string twin, HistoryUpdateRequestModel<VariantValue> request, CancellationToken ct) {
            var conn = await _registry.GetConnectionAsync(twin, ct: ct).ConfigureAwait(false);
            return await _history.HistoryUpdateAsync(conn, request, ct).ConfigureAwait(false);
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
        private readonly IBrowseServices<ConnectionModel> _browse;
        private readonly IHistoricAccessServices<ConnectionModel> _history;
    }
}
