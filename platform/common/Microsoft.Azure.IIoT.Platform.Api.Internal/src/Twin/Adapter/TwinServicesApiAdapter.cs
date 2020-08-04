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

    /// <summary>
    /// Implements node services as adapter on top of api.
    /// </summary>
    public sealed class TwinServicesApiAdapter : IBrowseServices<string>,
        INodeServices<string>, ITransferServices<string> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="client"></param>
        public TwinServicesApiAdapter(ITwinServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            string endpoint, BrowseRequestModel request) {
            var result = await _client.NodeBrowseFirstAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            string endpoint, BrowseNextRequestModel request) {
            var result = await _client.NodeBrowseNextAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            string endpoint, BrowsePathRequestModel request) {
            var result = await _client.NodeBrowsePathAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            string endpoint, ValueReadRequestModel request) {
            var result = await _client.NodeValueReadAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            string endpoint, ValueWriteRequestModel request) {
            var result = await _client.NodeValueWriteAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            string endpoint, MethodMetadataRequestModel request) {
            var result = await _client.NodeMethodGetMetadataAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            string endpoint, MethodCallRequestModel request) {
            var result = await _client.NodeMethodCallAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            string endpoint, ReadRequestModel request) {
            var result = await _client.NodeReadAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            string endpoint, WriteRequestModel request) {
            var result = await _client.NodeWriteAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResultModel> ModelUploadStartAsync(
            string endpoint, ModelUploadStartRequestModel request) {
            var result = await _client.ModelUploadStartAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublishStartResultModel> NodePublishStartAsync(
            string endpoint, PublishStartRequestModel request) {
            var result = await _client.NodePublishStartAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublishStopResultModel> NodePublishStopAsync(
            string endpoint, PublishStopRequestModel request) {
            var result = await _client.NodePublishStopAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublishBulkResultModel> NodePublishBulkAsync(
            string endpoint, PublishBulkRequestModel request) {
            var result = await _client.NodePublishBulkAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublishedItemListResultModel> NodePublishListAsync(
            string endpoint, PublishedItemListRequestModel request) {
            var result = await _client.NodePublishListAsync(endpoint,
                request.ToApiModel());
            return result.ToServiceModel();
        }

        private readonly ITwinServiceApi _client;
    }
}
