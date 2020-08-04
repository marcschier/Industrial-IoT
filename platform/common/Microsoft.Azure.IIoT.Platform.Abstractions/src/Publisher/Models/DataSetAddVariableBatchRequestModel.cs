// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A data set management operation that creates and new variables to a
    /// dataset in bulk and also allows modification of the dataset in
    /// a single operation.
    /// </summary>
    public class DataSetAddVariableBatchRequestModel {

        /// <summary>
        /// Variables to add to the dataset in the specified writer
        /// </summary>
        public List<DataSetAddVariableRequestModel> Variables { get; set; }

        /// <summary>
        /// (Optional) Update data source to use publishing interval
        /// </summary>
        public TimeSpan? DataSetPublishingInterval { get; set; }

        /// <summary>
        /// (Optional) Update data source to specified user
        /// </summary>
        public CredentialModel User { get; set; }
    }
}