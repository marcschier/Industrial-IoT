﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Result of a variable removal
    /// </summary>
    public class DataSetRemoveVariableBatchResultModel {

        /// <summary>
        /// Variables to remove from the dataset in the writer
        /// </summary>
        public IReadOnlyList<DataSetRemoveVariableResultModel> Results { get; set; }
    }
}