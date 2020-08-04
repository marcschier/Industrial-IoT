// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset Writer registry
    /// </summary>
    public interface IDataSetWriterRegistry {

        /// <summary>
        /// Register new dataset writer
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterAddResultModel> AddDataSetWriterAsync(
            DataSetWriterAddRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Read full dataset writer model which includes all
        /// dataset members if there are any.
        /// </summary>
        /// <param name="dataSetWriterId">The dataSetWriterId</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterModel> GetDataSetWriterAsync(
            string dataSetWriterId, CancellationToken ct = default);

        /// <summary>
        /// Update an existing application, e.g. server
        /// certificate, or additional capabilities.
        /// </summary>
        /// <param name="dataSetWriterId">The dataSetWriterId</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDataSetWriterAsync(string dataSetWriterId,
            DataSetWriterUpdateRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Register new event based dataset
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetAddEventResultModel> AddEventDataSetAsync(
            string dataSetWriterId, DataSetAddEventRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Read full event set if any.
        /// </summary>
        /// <param name="dataSetWriterId">The event set Id</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublishedDataSetEventsModel> GetEventDataSetAsync(
            string dataSetWriterId, CancellationToken ct = default);

        /// <summary>
        /// Update an existing event set.
        /// </summary>
        /// <param name="dataSetWriterId">The event set Id</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateEventDataSetAsync(string dataSetWriterId,
            DataSetUpdateEventRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Unregister eventset and remove from dataset.
        /// </summary>
        /// <param name="dataSetWriterId">The event set Id</param>
        /// <param name="generationId"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RemoveEventDataSetAsync(string dataSetWriterId,
            string generationId, PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Register new dataset variable
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetAddVariableResultModel> AddDataSetVariableAsync(
            string dataSetWriterId, DataSetAddVariableRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Update an existing variable info.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId">The variableId</param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDataSetVariableAsync(string dataSetWriterId,
            string variableId, DataSetUpdateVariableRequestModel request,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// List all dataset variables or continue find query.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublishedDataSetVariableListModel> ListDataSetVariablesAsync(
            string dataSetWriterId, string continuation = null,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Query variables in dataset.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublishedDataSetVariableListModel> QueryDataSetVariablesAsync(
            string dataSetWriterId, PublishedDataSetVariableQueryModel query,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Unregister dataset variable and remove from dataset.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="generationId"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RemoveDataSetVariableAsync(string dataSetWriterId,
            string variableId, string generationId,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// List all dataset writers or continue find query.
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoListModel> ListDataSetWritersAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find dataset writers for the specified information
        /// criterias.  The returned continuation if any must
        /// be passed to ListDataSetWritersAsync.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoListModel> QueryDataSetWritersAsync(
            DataSetWriterInfoQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Unregister dataset writer and linked items.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="generationId"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RemoveDataSetWriterAsync(string dataSetWriterId,
            string generationId,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);
    }
}
