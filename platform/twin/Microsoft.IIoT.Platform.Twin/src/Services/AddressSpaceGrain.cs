// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Grains {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Twin;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using Orleans;

    /// <summary>
    /// Implements addpress space and certificate services as actor grain
    /// </summary>
    public sealed class AddressSpaceGrain : Grain, IAddressSpaceGrain {

        /// <summary>
        /// Create twin grain
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="browse"></param>
        /// <param name="nodes"></param>
        /// <param name="certificates"></param>
        /// <param name="history"></param>
        public AddressSpaceGrain(ICertificateServices<ConnectionModel> certificates,
            IBrowseServices<ConnectionModel> browse, INodeServices<ConnectionModel> nodes,
            IHistoricAccessServices<ConnectionModel> history, ITwinRegistry registry) {

            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            _browse = browse ?? throw new ArgumentNullException(nameof(browse));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        /// <inheritdoc/>
        public override async Task OnActivateAsync() {
            _twin = await _registry.GetTwinAsync(
                GrainReference.GrainIdentity.PrimaryKeyString).ConfigureAwait(true);
            await base.OnActivateAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public override Task OnDeactivateAsync() {
            _twin = null; // Clear twin
            return base.OnDeactivateAsync();
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            HistoryReadRequestModel<VariantValue> request, CancellationToken ct) {
            return await _history.HistoryReadAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            HistoryReadNextRequestModel request, CancellationToken ct) {
            return await _history.HistoryReadNextAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            HistoryUpdateRequestModel<VariantValue> request, CancellationToken ct) {
            return await _history.HistoryUpdateAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            ValueReadRequestModel request, CancellationToken ct) {
            return await _nodes.NodeValueReadAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            ValueWriteRequestModel request, CancellationToken ct) {
            return await _nodes.NodeValueWriteAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            MethodMetadataRequestModel request, CancellationToken ct) {
            return await _nodes.NodeMethodGetMetadataAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            MethodCallRequestModel request, CancellationToken ct) {
            return await _nodes.NodeMethodCallAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            ReadRequestModel request, CancellationToken ct) {
            return await _nodes.NodeReadAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(WriteRequestModel request,
            CancellationToken ct) {
            return await _nodes.NodeWriteAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainModel> GetCertificateAsync(
            CancellationToken ct) {
            return await _certificates.GetCertificateAsync(_twin.Connection,
                ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(BrowseRequestModel request,
            CancellationToken ct) {
            return await _browse.NodeBrowseFirstAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            BrowseNextRequestModel request, CancellationToken ct) {
            return await _browse.NodeBrowseNextAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            BrowsePathRequestModel request, CancellationToken ct) {
            return await _browse.NodeBrowsePathAsync(_twin.Connection,
                request, ct).ConfigureAwait(true);
        }

        private readonly ITwinRegistry _registry;
        private readonly INodeServices<ConnectionModel> _nodes;
        private readonly ICertificateServices<ConnectionModel> _certificates;
        private readonly IBrowseServices<ConnectionModel> _browse;
        private readonly IHistoricAccessServices<ConnectionModel> _history;
        private TwinModel _twin;
    }
}
