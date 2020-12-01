// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin {
    using Microsoft.IIoT.Platform.Twin.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Historian services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHistorianServices<T> {

        /// <summary>
        /// Replace events
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryReplaceEventsAsync(T twin,
            HistoryUpdateRequestModel<ReplaceEventsDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Insert events
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryInsertEventsAsync(T twin,
            HistoryUpdateRequestModel<InsertEventsDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Delete events
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryDeleteEventsAsync(T twin,
            HistoryUpdateRequestModel<DeleteEventsDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Delete values at specified times
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryDeleteValuesAtTimesAsync(T twin,
            HistoryUpdateRequestModel<DeleteValuesAtTimesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Delete modified values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryDeleteModifiedValuesAsync(T twin,
            HistoryUpdateRequestModel<DeleteModifiedValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Delete values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryDeleteValuesAsync(T twin,
            HistoryUpdateRequestModel<DeleteValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Replace values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryReplaceValuesAsync(T twin,
            HistoryUpdateRequestModel<ReplaceValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Insert values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryUpdateResultModel> HistoryInsertValuesAsync(T twin,
            HistoryUpdateRequestModel<InsertValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read historic events
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<HistoricEventModel[]>> HistoryReadEventsAsync(
            T twin, HistoryReadRequestModel<ReadEventsDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read next set of events
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadNextResultModel<HistoricEventModel[]>> HistoryReadEventsNextAsync(
            T twin, HistoryReadNextRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Read historic values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAsync(
            T twin, HistoryReadRequestModel<ReadValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read historic values at times
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadValuesAtTimesAsync(
            T twin, HistoryReadRequestModel<ReadValuesAtTimesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read processed historic values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadProcessedValuesAsync(
            T twin, HistoryReadRequestModel<ReadProcessedValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read modified values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadResultModel<HistoricValueModel[]>> HistoryReadModifiedValuesAsync(
            T twin, HistoryReadRequestModel<ReadModifiedValuesDetailsModel> request,
            CancellationToken ct = default);

        /// <summary>
        /// Read next set of historic values
        /// </summary>
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HistoryReadNextResultModel<HistoricValueModel[]>> HistoryReadValuesNextAsync(
            T twin, HistoryReadNextRequestModel request,
            CancellationToken ct = default);
    }
}
