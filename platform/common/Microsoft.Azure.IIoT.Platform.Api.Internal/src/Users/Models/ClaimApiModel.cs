// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Instance of Claim.
    /// </summary>
    [DataContract]
    public class ClaimApiModel {

        /// <summary>
        /// Claim Type.
        /// </summary>
        [DataMember(Name = "type", Order = 0)]
        public string Type { get; set; }

        /// <summary>
        /// Claim Value.
        /// </summary>
        [DataMember(Name = "value", Order = 1)]
        public string Value { get; set; }

        /// <summary>
        /// Gets the value type of the claim.
        /// </summary>
        [DataMember(Name = "valueType", Order = 2)]
        public string ValueType { get; set; }
    }
}