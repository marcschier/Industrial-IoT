// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Request list of published items
    /// </summary>
    [DataContract]
    public class PublishedItemListRequestApiModel {

        /// <summary>
        /// Continuation token or null to start
        /// </summary>
        [DataMember(Name = "continuationToken", Order = 0,
            EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }
    }
}
