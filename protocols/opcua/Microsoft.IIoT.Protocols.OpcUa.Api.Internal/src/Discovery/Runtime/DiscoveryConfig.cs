// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Runtime {
    using Microsoft.IIoT.Api.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration - wraps a configuration root
    /// </summary>
    public class DiscoveryConfig : ApiConfigBase, IDiscoveryConfig {

        /// <summary>
        /// Registry configuration
        /// </summary>
        private const string kDiscoveryServiceUrlKey = "DiscoveryServiceUrl";

        /// <summary>OPC registry endpoint url</summary>
        public string DiscoveryServiceUrl => GetStringOrDefault(
            kDiscoveryServiceUrlKey,
            GetStringOrDefault(PcsVariable.PCS_DISCOVERY_SERVICE_URL,
                GetDefaultUrl("9042", "registry")));

        /// <inheritdoc/>
        public DiscoveryConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
