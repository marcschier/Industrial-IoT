// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Clients {
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.OpcUa;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Adapts historian services to historic access services
    /// </summary>
    public sealed class HistorianServicesAdapter<T> : IHistorianServices<T> {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="codec"></param>
        public HistorianServicesAdapter(IHistoricAccessServices<T> client, 
            IVariantEncoderFactory codec) {
            _codec = codec?.Default ?? throw new ArgumentNullException(nameof(codec));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryDeleteEventsAsync(
            T twin, HistoryUpdateRequestModel<DeleteEventsDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryDeleteValuesAtTimesAsync(
            T twin, HistoryUpdateRequestModel<DeleteValuesAtTimesDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryDeleteModifiedValuesAsync(
            T twin, HistoryUpdateRequestModel<DeleteModifiedValuesDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryDeleteValuesAsync(
            T twin, HistoryUpdateRequestModel<DeleteValuesDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryReplaceEventsAsync(
            T twin, HistoryUpdateRequestModel<ReplaceEventsDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryReplaceValuesAsync(
            T twin, HistoryUpdateRequestModel<ReplaceValuesDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryInsertEventsAsync(
            T twin, HistoryUpdateRequestModel<InsertEventsDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public Task<HistoryUpdateResultModel> HistoryInsertValuesAsync(
            T twin, HistoryUpdateRequestModel<InsertValuesDetailsModel> request,
            CancellationToken ct) {
            return _client.HistoryUpdateAsync(twin, request.ToRawModel(_codec.Encode), ct);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricEventModel[]>> HistoryReadEventsAsync(
            T twin, HistoryReadRequestModel<ReadEventsDetailsModel> request,
            CancellationToken ct) {
            var results = await _client.HistoryReadAsync(twin, request.ToRawModel(_codec.Encode), 
                ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeEvents);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<HistoricEventModel[]>> HistoryReadEventsNextAsync(
            T twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var results = await _client.HistoryReadNextAsync(twin, request, ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeEvents);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAsync(
            T twin, HistoryReadRequestModel<ReadValuesDetailsModel> request,
            CancellationToken ct) {
            var results = await _client.HistoryReadAsync(twin, request.ToRawModel(_codec.Encode),
                ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeValues);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAtTimesAsync(
            T twin, HistoryReadRequestModel<ReadValuesAtTimesDetailsModel> request,
            CancellationToken ct) {
            var results = await _client.HistoryReadAsync(twin, request.ToRawModel(_codec.Encode), 
                ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeValues);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadProcessedValuesAsync(
            T twin, HistoryReadRequestModel<ReadProcessedValuesDetailsModel> request,
            CancellationToken ct) {
            var results = await _client.HistoryReadAsync(twin, request.ToRawModel(_codec.Encode),
                ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeValues);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadModifiedValuesAsync(
            T twin, HistoryReadRequestModel<ReadModifiedValuesDetailsModel> request,
            CancellationToken ct) {
            var results = await _client.HistoryReadAsync(twin, request.ToRawModel(_codec.Encode), 
                ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeValues);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<HistoricValueModel[]>> HistoryReadValuesNextAsync(
            T twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var results = await _client.HistoryReadNextAsync(twin, request, ct).ConfigureAwait(false);
            return results.ToSpecificModel(_codec.DecodeValues);
        }

        private readonly IVariantEncoder _codec;
        private readonly IHistoricAccessServices<T> _client;
    }
}
