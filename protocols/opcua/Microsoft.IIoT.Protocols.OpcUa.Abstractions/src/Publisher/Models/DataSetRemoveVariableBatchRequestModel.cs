// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {
    using System.Collections.Generic;

    /// <summary>
    /// A data set management operation that removes variables from a
    /// dataset in bulk.
    /// </summary>
    public class DataSetRemoveVariableBatchRequestModel {

        /// <summary>
        /// Variables to add to the dataset in the specified writer
        /// </summary>
        public IReadOnlyList<DataSetRemoveVariableRequestModel> Variables { get; set; }
    }
}