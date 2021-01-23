// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Discovery cancel request
    /// </summary>
    [DataContract]
    public class DiscoveryCancelInternalApiModel : DiscoveryCancelApiModel {

        /// <summary>
        /// Operation audit context
        /// </summary>
        [DataMember(Name = "context", Order = 10,
            EmitDefaultValue = false)]
        public OperationContextApiModel Context { get; set; }
    }
}
