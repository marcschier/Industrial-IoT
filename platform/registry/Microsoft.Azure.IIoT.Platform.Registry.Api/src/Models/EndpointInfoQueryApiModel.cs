// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Endpoint query
    /// </summary>
    [DataContract]
    public class EndpointInfoQueryApiModel {

        /// <summary>
        /// Endoint url for direct server access
        /// </summary>
        [DataMember(Name = "url", Order = 0,
            EmitDefaultValue = false)]
        public string Url { get; set; }

        /// <summary>
        /// Endpoint certificate thumbprint
        /// </summary>
        [DataMember(Name = "certificate", Order = 1,
            EmitDefaultValue = false)]
        public string Certificate { get; set; }

        /// <summary>
        /// Security Mode
        /// </summary>
        [DataMember(Name = "securityMode", Order = 2,
            EmitDefaultValue = false)]
        public SecurityMode? SecurityMode { get; set; }

        /// <summary>
        /// Security policy uri
        /// </summary>
        [DataMember(Name = "securityPolicy", Order = 3,
            EmitDefaultValue = false)]
        public string SecurityPolicy { get; set; }

        /// <summary>
        /// Whether to test for visibility
        /// </summary>
        [DataMember(Name = "visibility", Order = 4,
            EmitDefaultValue = false)]
        public EntityVisibility? Visibility { get; set; }

        /// <summary>
        /// Discoverer id to filter with
        /// </summary>
        [DataMember(Name = "discovererId", Order = 5,
            EmitDefaultValue = false)]
        public string DiscovererId { get; set; }

        /// <summary>
        /// Application id to filter
        /// </summary>
        [DataMember(Name = "applicationId", Order = 6,
            EmitDefaultValue = false)]
        public string ApplicationId { get; set; }
    }
}

