// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Application registration request
    /// </summary>
    public class ApplicationRegistrationRequestModel {

        /// <summary>
        /// Application uri
        /// </summary>
        public string ApplicationUri { get; set; }

        /// <summary>
        /// Type of application
        /// </summary>
        public ApplicationType? ApplicationType { get; set; }

        /// <summary>
        /// Product uri
        /// </summary>
        public string ProductUri { get; set; }

        /// <summary>
        /// Default name of application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Locale of name - defaults to "en"
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Localizations
        /// </summary>
        public IReadOnlyDictionary<string, string> LocalizedNames { get; set; }

        /// <summary>
        /// Application capabilities
        /// </summary>
        public IReadOnlySet<string> Capabilities { get; set; }

        /// <summary>
        /// Discovery urls of the application
        /// </summary>
        public IReadOnlySet<string> DiscoveryUrls { get; set; }

        /// <summary>
        /// Discovery profile uri
        /// </summary>
        public string DiscoveryProfileUri { get; set; }

        /// <summary>
        /// Gateway server uri
        /// </summary>
        public string GatewayServerUri { get; set; }
    }
}
