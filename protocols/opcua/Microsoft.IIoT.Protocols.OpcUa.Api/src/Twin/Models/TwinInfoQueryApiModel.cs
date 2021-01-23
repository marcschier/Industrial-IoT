// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin query
    /// </summary>
    [DataContract]
    public class TwinInfoQueryApiModel {

        /// <summary>
        /// Endpoint id
        /// </summary>
        [DataMember(Name = "endpointId", Order = 0,
            EmitDefaultValue = false)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Credential type for the connection 
        /// </summary>
        [DataMember(Name = "credential", Order = 1,
            EmitDefaultValue = false)]
        public CredentialType? Credential { get; set; }

        /// <summary>
        /// The last state of the the connection
        /// </summary>
        [DataMember(Name = "state", Order = 2,
            EmitDefaultValue = false)]
        public ConnectionStatus? State { get; set; }
    }
}

