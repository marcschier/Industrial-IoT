// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset bulk operation
    /// </summary>
    public interface IDataSetBatchOperations {

        /// <summary>
        /// Add variables to a dataset writer in bulk.
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetAddVariableBatchResultModel> AddVariablesToDataSetWriterAsync(
            string dataSetWriterId, DataSetAddVariableBatchRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Add variables on endpoint in bulk
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetAddVariableBatchResultModel> AddVariablesToDefaultDataSetWriterAsync(
            string endpointId, DataSetAddVariableBatchRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Remove variables from dataset in bulk
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetRemoveVariableBatchResultModel> RemoveVariablesFromDataSetWriterAsync(
            string dataSetWriterId, DataSetRemoveVariableBatchRequestModel request,
            OperationContextModel context = null,
            CancellationToken ct = default);
    }
}
