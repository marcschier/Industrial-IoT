// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows update of data set writer and dataset state
    /// </summary>
    public interface IDataSetWriterStateUpdate {

        /// <summary>
        /// Update variable monitored item state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDataSetVariableStateAsync(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Update event monitored item state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDataSetEventStateAsync(string dataSetWriterId,
            PublishedDataSetItemStateModel state,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);

        /// <summary>
        /// Update dataset writer subscription state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDataSetWriterStateAsync(string dataSetWriterId,
            PublishedDataSetSourceStateModel state,
            PublisherOperationContextModel context = null,
            CancellationToken ct = default);
    }
}