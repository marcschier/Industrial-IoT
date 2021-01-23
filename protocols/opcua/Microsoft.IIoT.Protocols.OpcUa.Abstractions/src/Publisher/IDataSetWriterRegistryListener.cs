// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Notified when writers change
    /// </summary>
    public interface IDataSetWriterRegistryListener {

        /// <summary>
        /// New dataset writer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        Task OnDataSetWriterAddedAsync(OperationContextModel context,
            DataSetWriterInfoModel dataSetWriter);

        /// <summary>
        /// Called when writer was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        Task OnDataSetWriterUpdatedAsync(OperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter = null);

        /// <summary>
        /// Called when writer state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        Task OnDataSetWriterStateChangeAsync(OperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter = null);

        /// <summary>
        /// Called when writer was deleted which implies all
        /// dataset items were also deleted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        Task OnDataSetWriterRemovedAsync(OperationContextModel context,
            DataSetWriterInfoModel dataSetWriter);
    }
}
