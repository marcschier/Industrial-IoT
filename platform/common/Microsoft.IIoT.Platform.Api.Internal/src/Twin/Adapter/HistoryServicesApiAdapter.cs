// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api {
    using Microsoft.IIoT.Platform.Twin.Api.Models;
    using Microsoft.IIoT.Platform.Twin.Models;
    using Microsoft.IIoT.Platform.Twin;
    using System;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Implements historian services as adapter on top of api.
    /// </summary>
    public sealed class HistoryServicesApiAdapter : IHistorianServices<string> {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        public HistoryServicesApiAdapter(IHistoryServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryReplaceEventsAsync(
            string twin, HistoryUpdateRequestModel<ReplaceEventsDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReplaceEventsAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryInsertEventsAsync(
            string twin, HistoryUpdateRequestModel<InsertEventsDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryInsertEventsAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryDeleteEventsAsync(
            string twin, HistoryUpdateRequestModel<DeleteEventsDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryDeleteEventsAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryDeleteValuesAtTimesAsync(
            string twin, HistoryUpdateRequestModel<DeleteValuesAtTimesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryDeleteValuesAtTimesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryDeleteModifiedValuesAsync(
            string twin, HistoryUpdateRequestModel<DeleteModifiedValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryDeleteModifiedValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryDeleteValuesAsync(
            string twin, HistoryUpdateRequestModel<DeleteValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryDeleteValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryReplaceValuesAsync(
            string twin, HistoryUpdateRequestModel<ReplaceValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReplaceValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryInsertValuesAsync(
            string twin, HistoryUpdateRequestModel<InsertValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryInsertValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricEventModel[]>> HistoryReadEventsAsync(
            string twin, HistoryReadRequestModel<ReadEventsDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReadEventsAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<HistoricEventModel[]>> HistoryReadEventsNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var result = await _client.HistoryReadEventsNextAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAsync(
            string twin, HistoryReadRequestModel<ReadValuesDetailsModel> request, CancellationToken ct) {
            var result = await _client.HistoryReadValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAtTimesAsync(
            string twin, HistoryReadRequestModel<ReadValuesAtTimesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReadValuesAtTimesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadProcessedValuesAsync(
            string twin, HistoryReadRequestModel<ReadProcessedValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReadProcessedValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadModifiedValuesAsync(
            string twin, HistoryReadRequestModel<ReadModifiedValuesDetailsModel> request,
            CancellationToken ct) {
            var result = await _client.HistoryReadModifiedValuesAsync(twin,
                request.ToApiModel(m => m.ToApiModel()), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<HistoricValueModel[]>> HistoryReadValuesNextAsync(
            string twin, HistoryReadNextRequestModel request, CancellationToken ct) {
            var result = await _client.HistoryReadValuesNextAsync(twin,
                request.ToApiModel(), ct).ConfigureAwait(false);
            return result.ToServiceModel(m => m?.Select(x => x.ToServiceModel()).ToArray());
        }

        private readonly IHistoryServiceApi _client;
    }
}
