// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Result of a variable registration
    /// </summary>
    [DataContract]
    public class DataSetAddVariableBatchResponseApiModel {

        /// <summary>
        /// Variables to add to the dataset in the writer
        /// </summary>
        [DataMember(Name = "results", Order = 0)]
        public List<DataSetAddVariableResponseApiModel> Results { get; set; }
    }
}