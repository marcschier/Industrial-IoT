// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// List of data set variables
    /// </summary>
    [DataContract]
    public class PublishedDataSetVariableListApiModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        [DataMember(Name = "continuationToken", Order = 0,
            EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Variables
        /// </summary>
        [DataMember(Name = "variables", Order = 1,
            EmitDefaultValue = false)]
        public List<PublishedDataSetVariableApiModel> Variables { get; set; }
    }
}