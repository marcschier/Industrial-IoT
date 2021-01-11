// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Clients {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Extensions.Orleans;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements addpress space and certificate services as grain client
    /// </summary>
    public sealed class AddressSpaceGrainClient : IBrowseServices<string>,
        ICertificateServices<string>, INodeServices<string>,
        IHistoricAccessServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="client"></param>
        public AddressSpaceGrainClient(IOrleansGrainClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            string twin, BrowseRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeBrowseFirstAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            string twin, BrowseNextRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeBrowseNextAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            string twin, BrowsePathRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeBrowsePathAsync(request, ct).ConfigureAwait(true);
        }
        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            string twin, HistoryReadRequestModel<VariantValue> request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.HistoryReadAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.HistoryReadNextAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            string twin, HistoryUpdateRequestModel<VariantValue> request,
            CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.HistoryUpdateAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            string twin, ValueReadRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeValueReadAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            string twin, ValueWriteRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeValueWriteAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            string twin, MethodMetadataRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeMethodGetMetadataAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            string twin, MethodCallRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeMethodCallAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            string twin, ReadRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeReadAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            string twin, WriteRequestModel request, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.NodeWriteAsync(request, ct).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainModel> GetCertificateAsync(
            string twin, CancellationToken ct) {
            var grain = _client.Grains.GetGrain<IAddressSpaceGrain>(twin);
            return await grain.GetCertificateAsync(ct).ConfigureAwait(true);
        }

        private readonly IOrleansGrainClient _client;
    }
}
