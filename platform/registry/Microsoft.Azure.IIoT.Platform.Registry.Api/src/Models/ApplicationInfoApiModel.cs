// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Application info model
    /// </summary>
    [DataContract]
    public class ApplicationInfoApiModel {

        /// <summary>
        /// Unique application id
        /// </summary>
        [DataMember(Name = "applicationId", Order = 0)]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Type of application
        /// </summary>
        /// <example>Server</example>
        [DataMember(Name = "applicationType", Order = 1)]
        public ApplicationType ApplicationType { get; set; }

        /// <summary>
        /// Unique application uri
        /// </summary>
        [DataMember(Name = "applicationUri", Order = 2)]
        public string ApplicationUri { get; set; }

        /// <summary>
        /// Product uri
        /// </summary>
        [DataMember(Name = "productUri", Order = 3,
            EmitDefaultValue = false)]
        public string ProductUri { get; set; }

        /// <summary>
        /// Default name of application
        /// </summary>
        [DataMember(Name = "applicationName", Order = 4,
            EmitDefaultValue = false)]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Locale of default name - defaults to "en"
        /// </summary>
        [DataMember(Name = "locale", Order = 5,
            EmitDefaultValue = false)]
        public string Locale { get; set; }

        /// <summary>
        /// Localized Names of application keyed on locale
        /// </summary>
        [DataMember(Name = "localizedNames", Order = 6,
            EmitDefaultValue = false)]
        public IReadOnlyDictionary<string, string> LocalizedNames { get; set; }

        /// <summary>
        /// The capabilities advertised by the server.
        /// </summary>
        /// <example>LDS</example>
        /// <example>DA</example>
        [DataMember(Name = "capabilities", Order = 7,
            EmitDefaultValue = false)]
        public IReadOnlySet<string> Capabilities { get; set; }

        /// <summary>
        /// Discovery urls of the server
        /// </summary>
        [DataMember(Name = "discoveryUrls", Order = 8,
            EmitDefaultValue = false)]
        public IReadOnlySet<string> DiscoveryUrls { get; set; }

        /// <summary>
        /// Discovery profile uri
        /// </summary>
        [DataMember(Name = "discoveryProfileUri", Order = 9,
            EmitDefaultValue = false)]
        public string DiscoveryProfileUri { get; set; }

        /// <summary>
        /// Gateway server uri
        /// </summary>
        [DataMember(Name = "gatewayServerUri", Order = 10,
            EmitDefaultValue = false)]
        public string GatewayServerUri { get; set; }

        /// <summary>
        /// Host addresses of server application or null
        /// </summary>
        [DataMember(Name = "hostAddresses", Order = 11,
            EmitDefaultValue = false)]
        public IReadOnlySet<string> HostAddresses { get; set; }

        /// <summary>
        /// Discoverer that registered the application
        /// </summary>
        [DataMember(Name = "discovererId", Order = 13,
            EmitDefaultValue = false)]
        public string DiscovererId { get; set; }

        /// <summary>
        /// Application visibility
        /// </summary>
        [DataMember(Name = "visibility", Order = 14,
            EmitDefaultValue = false)]
        public EntityVisibility? Visibility { get; set; }

        /// <summary>
        /// Last time application was seen
        /// </summary>
        [DataMember(Name = "notSeenSince", Order = 15,
            EmitDefaultValue = false)]
        public DateTime? NotSeenSince { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 16,
            EmitDefaultValue = false)]
        public RegistryOperationApiModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        [DataMember(Name = "updated", Order = 17,
            EmitDefaultValue = false)]
        public RegistryOperationApiModel Updated { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 18,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}
