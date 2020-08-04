// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// A data set management operation that removes variables from a
    /// dataset in bulk.
    /// </summary>
    [DataContract]
    public class DataSetRemoveVariableBatchRequestApiModel {

        /// <summary>
        /// Variables to add to the dataset in the specified writer
        /// </summary>
        [DataMember(Name = "variables", Order = 0)]
        public List<DataSetRemoveVariableRequestApiModel> Variables { get; set; }
    }
}