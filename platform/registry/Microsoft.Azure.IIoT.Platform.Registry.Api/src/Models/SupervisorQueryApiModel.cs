// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Supervisor registration query
    /// </summary>
    [DataContract]
    public class SupervisorQueryApiModel {

        /// <summary>
        /// Managing provided endpoint twin
        /// </summary>
        [DataMember(Name = "endpointId", Order = 1,
            EmitDefaultValue = false)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Included connected or disconnected
        /// </summary>
        [DataMember(Name = "connected", Order = 2,
            EmitDefaultValue = false)]
        public bool? Connected { get; set; }
    }
}
