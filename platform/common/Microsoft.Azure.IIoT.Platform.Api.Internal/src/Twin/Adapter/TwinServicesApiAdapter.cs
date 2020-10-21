// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements node services as adapter on top of api.
    /// </summary>
    public sealed class TwinServicesApiAdapter : IBrowseServices<string>,
        INodeServices<string>, ITransferServices<string>, ITwinRegistry {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="client"></param>
        public TwinServicesApiAdapter(ITwinServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<TwinActivationResultModel> ActivateTwinAsync(
            TwinActivationRequestModel request, CancellationToken ct) {
            var result = await _client.ActivateTwinAsync(
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListModel> ListTwinsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListTwinsAsync(
                continuation, pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListModel> QueryTwinsAsync(
            TwinInfoQueryModel query, int? pageSize, CancellationToken ct) {
            var result = await _client.QueryTwinsAsync(query.ToApiModel(), 
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<TwinModel> GetTwinAsync(string twin, 
            CancellationToken ct) {
            var result = await _client.GetTwinAsync(twin,
                ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdateTwinAsync(string twin, TwinInfoUpdateModel 
            request, CancellationToken ct) {
            await _client.UpdateTwinAsync(twin, request.ToApiModel(),
                ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DectivateTwinAsync(string twin, string generationId,
            CancellationToken ct) {
            await _client.DectivateTwinAsync(twin, generationId, 
                ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            string twin, BrowseRequestModel request, CancellationToken ct) {
            var result = await _client.NodeBrowseFirstAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            string twin, BrowseNextRequestModel request, CancellationToken ct) {
            var result = await _client.NodeBrowseNextAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            string twin, BrowsePathRequestModel request, CancellationToken ct) {
            var result = await _client.NodeBrowsePathAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            string twin, ValueReadRequestModel request, CancellationToken ct) {
            var result = await _client.NodeValueReadAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            string twin, ValueWriteRequestModel request, CancellationToken ct) {
            var result = await _client.NodeValueWriteAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            string twin, MethodMetadataRequestModel request, CancellationToken ct) {
            var result = await _client.NodeMethodGetMetadataAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            string twin, MethodCallRequestModel request, CancellationToken ct) {
            var result = await _client.NodeMethodCallAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            string twin, ReadRequestModel request, CancellationToken ct) {
            var result = await _client.NodeReadAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            string twin, WriteRequestModel request, CancellationToken ct) {
            var result = await _client.NodeWriteAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            string twin, ModelUploadStartRequestModel request, CancellationToken ct) {
            var result = await _client.ModelUploadStartAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        private readonly ITwinServiceApi _client;
    }
}
