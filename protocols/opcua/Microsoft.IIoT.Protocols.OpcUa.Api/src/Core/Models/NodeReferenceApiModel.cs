// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models {
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// reference model
    /// </summary>
    [DataContract]
    public class NodeReferenceApiModel {

        /// <summary>
        /// Reference Type id
        /// </summary>
        [DataMember(Name = "referenceTypeId", Order = 0,
            EmitDefaultValue = false)]
        public string ReferenceTypeId { get; set; }

        /// <summary>
        /// Browse direction of reference
        /// </summary>
        [DataMember(Name = "direction", Order = 1,
            EmitDefaultValue = false)]
        public BrowseDirection? Direction { get; set; }

        /// <summary>
        /// Target node
        /// </summary>
        [DataMember(Name = "target", Order = 2)]
        [Required]
        public NodeApiModel Target { get; set; }
    }
}
