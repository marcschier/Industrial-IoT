// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Application information query
    /// </summary>
    [DataContract]
    public class ApplicationInfoQueryApiModel {

        /// <summary>
        /// Type of application
        /// </summary>
        [DataMember(Name = "applicationType", Order = 0,
            EmitDefaultValue = false)]
        public ApplicationType? ApplicationType { get; set; }

        /// <summary>
        /// Application uri
        /// </summary>
        [DataMember(Name = "applicationUri", Order = 1,
            EmitDefaultValue = false)]
        public string ApplicationUri { get; set; }

        /// <summary>
        /// Product uri
        /// </summary>
        [DataMember(Name = "productUri", Order = 2,
            EmitDefaultValue = false)]
        public string ProductUri { get; set; }

        /// <summary>
        /// Name of application
        /// </summary>
        [DataMember(Name = "applicationName", Order = 3,
            EmitDefaultValue = false)]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Locale of application name - default is "en"
        /// </summary>
        [DataMember(Name = "locale", Order = 4,
            EmitDefaultValue = false)]
        public string Locale { get; set; }

        /// <summary>
        /// Application capability to query with
        /// </summary>
        [DataMember(Name = "capability", Order = 5,
            EmitDefaultValue = false)]
        public string Capability { get; set; }

        /// <summary>
        /// Discovery profile uri
        /// </summary>
        [DataMember(Name = "discoveryProfileUri", Order = 6,
            EmitDefaultValue = false)]
        public string DiscoveryProfileUri { get; set; }

        /// <summary>
        /// Gateway server uri
        /// </summary>
        [DataMember(Name = "gatewayServerUri", Order = 7,
            EmitDefaultValue = false)]
        public string GatewayServerUri { get; set; }

        /// <summary>
        /// Whether to test for visibility
        /// </summary>
        [DataMember(Name = "visibility", Order = 9,
            EmitDefaultValue = false)]
        public EntityVisibility? Visibility { get; set; }

        /// <summary>
        /// Discoverer id to filter with
        /// </summary>
        [DataMember(Name = "discovererId", Order = 10,
            EmitDefaultValue = false)]
        public string DiscovererId { get; set; }
    }
}

