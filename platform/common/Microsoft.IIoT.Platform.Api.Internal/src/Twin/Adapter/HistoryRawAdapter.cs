// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api {
    using Microsoft.IIoT.Platform.Twin.Api.Models;
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Twin;
    using Microsoft.IIoT.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implements historic access services as adapter on top of api.
    /// </summary>
    public sealed class HistoryRawAdapter : IHistoricAccessServices<string> {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        public HistoryRawAdapter(ITwinServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            string twin, HistoryReadRequestModel<VariantValue> request, 
            CancellationToken ct) {
            var result = await _client.HistoryReadRawAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var result = await _client.HistoryReadRawNextAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            string twin, HistoryUpdateRequestModel<VariantValue> request,
            CancellationToken ct) {
            var result = await _client.HistoryUpdateRawAsync(twin, 
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        private readonly ITwinServiceApi _client;
    }
}
