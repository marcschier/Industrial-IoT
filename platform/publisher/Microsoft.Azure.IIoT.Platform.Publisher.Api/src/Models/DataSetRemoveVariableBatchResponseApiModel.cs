// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Result of a variable removal
    /// </summary>
    [DataContract]
    public class DataSetRemoveVariableBatchResponseApiModel {

        /// <summary>
        /// Variables to remove from the dataset in the writer
        /// </summary>
        [DataMember(Name = "results", Order = 0)]
        public List<DataSetRemoveVariableResponseApiModel> Results { get; set; }
    }
}