// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Result of an twin activation
    /// </summary>
    [DataContract]
    public class TwinActivationResponseApiModel {

        /// <summary>
        /// New id twin was activated as
        /// </summary>
        [DataMember(Name = "id", Order = 0)]
        public string Id { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 1)]
        public string GenerationId { get; set; }
    }
}
