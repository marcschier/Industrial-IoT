// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// A data set variable removal request
    /// </summary>
    [DataContract]
    public class DataSetRemoveVariableRequestApiModel {

        /// <summary>
        /// Node id of the variable(s) to unregister
        /// </summary>
        [DataMember(Name = "publishedVariableNodeId", Order = 0)]
        public string PublishedVariableNodeId { get; set; }
    }
}