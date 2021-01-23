// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;

    /// <summary>
    /// Report state for dataset and dataset writer
    /// </summary>
    public interface IDataSetWriterStateReporter {

        /// <summary>
        /// Report variable monitored item state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void OnDataSetVariableStateChange(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state);

        /// <summary>
        /// Report event monitored item state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void OnDataSetEventStateChange(string dataSetWriterId,
            PublishedDataSetItemStateModel state);

        /// <summary>
        /// Report dataset writer subscription state
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void OnDataSetWriterStateChange(string dataSetWriterId,
            PublishedDataSetSourceStateModel state);
    }
}