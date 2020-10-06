// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Application registration update request
    /// </summary>
    [DataContract]
    public class ApplicationInfoUpdateApiModel {

        /// <summary>
        /// Product uri
        /// </summary>
        [DataMember(Name = "productUri", Order = 0,
            EmitDefaultValue = false)]
        public string ProductUri { get; set; }

        /// <summary>
        /// Default name of the server or client.
        /// </summary>
        [DataMember(Name = "applicationName", Order = 1,
            EmitDefaultValue = false)]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Locale of default name - defaults to "en"
        /// </summary>
        [DataMember(Name = "locale", Order = 2,
            EmitDefaultValue = false)]
        public string Locale { get; set; }

        /// <summary>
        /// Localized names keyed off locale id.
        /// To remove entry, set value for locale id to null.
        /// </summary>
        [DataMember(Name = "localizedNames", Order = 3,
            EmitDefaultValue = false)]
        public IReadOnlyDictionary<string, string> LocalizedNames { get; set; }

        /// <summary>
        /// Capabilities of the application
        /// </summary>
        [DataMember(Name = "capabilities", Order = 4,
            EmitDefaultValue = false)]
        public IReadOnlySet<string> Capabilities { get; set; }

        /// <summary>
        /// Discovery urls of the application
        /// </summary>
        [DataMember(Name = "discoveryUrls", Order = 5,
            EmitDefaultValue = false)]
        public IReadOnlySet<string> DiscoveryUrls { get; set; }

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
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 8,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}
