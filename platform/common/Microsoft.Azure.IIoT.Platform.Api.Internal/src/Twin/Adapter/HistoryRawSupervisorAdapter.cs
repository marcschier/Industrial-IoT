// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api {
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements historic access services as adapter on top of supervisor api.
    /// </summary>
    public sealed class HistoryRawSupervisorAdapter : IHistoricAccessServices<EndpointApiModel> {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="client"></param>
        public HistoryRawSupervisorAdapter(IHistoryModuleApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            EndpointApiModel endpoint, HistoryReadRequestModel<VariantValue> request) {
            var result = await _client.HistoryReadRawAsync(endpoint, request.ToApiModel()).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            EndpointApiModel endpoint, HistoryReadNextRequestModel request) {
            var result = await _client.HistoryReadRawNextAsync(endpoint, request.ToApiModel()).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            EndpointApiModel endpoint, HistoryUpdateRequestModel<VariantValue> request) {
            var result = await _client.HistoryUpdateRawAsync(endpoint, request.ToApiModel()).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        private readonly IHistoryModuleApi _client;
    }
}
