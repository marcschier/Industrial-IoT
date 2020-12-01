// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Discoverer registration query
    /// </summary>
    [DataContract]
    public class DiscovererQueryApiModel {

        /// <summary>
        /// Included connected or disconnected
        /// </summary>
        [DataMember(Name = "connected", Order = 2,
            EmitDefaultValue = false)]
        public bool? Connected { get; set; }
    }
}
