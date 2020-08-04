// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A data set management operation that creates and new variables to a
    /// dataset in bulk and also allows modification of the dataset in
    /// a single operation.
    /// </summary>
    [DataContract]
    public class DataSetAddVariableBatchRequestApiModel {

        /// <summary>
        /// Variables to add to the dataset in the specified writer
        /// </summary>
        [DataMember(Name = "variables", Order = 0)]
        public List<DataSetAddVariableRequestApiModel> Variables { get; set; }

        /// <summary>
        /// (Optional) Update data source to use publishing interval
        /// </summary>
        [DataMember(Name = "dataSetPublishingInterval", Order = 1,
            EmitDefaultValue = false)]
        public TimeSpan? DataSetPublishingInterval { get; set; }

        /// <summary>
        /// (Optional) Update data source to specified user
        /// </summary>
        [DataMember(Name = "user", Order = 2,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }
    }
}