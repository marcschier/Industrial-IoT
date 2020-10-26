// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Discovery request
    /// </summary>
    [DataContract]
    public class DiscoveryRequestInternalApiModel : DiscoveryRequestApiModel {

        /// <summary>
        /// Operation audit context
        /// </summary>
        [DataMember(Name = "context", Order = 10,
            EmitDefaultValue = false)]
        public OperationContextApiModel Context { get; set; }
    }
}
