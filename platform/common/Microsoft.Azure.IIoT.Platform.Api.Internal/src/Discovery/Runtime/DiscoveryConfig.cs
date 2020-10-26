// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Api.Runtime {
    using Microsoft.Azure.IIoT.Api.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration - wraps a configuration root
    /// </summary>
    public class DiscoveryConfig : ApiConfigBase, IDiscoveryConfig {

        /// <summary>
        /// Registry configuration
        /// </summary>
        private const string kOpcUaRegistryServiceUrlKey = "OpcRegistryServiceUrl";

        /// <summary>OPC registry endpoint url</summary>
        public string OpcUaRegistryServiceUrl => GetStringOrDefault(
            kOpcUaRegistryServiceUrlKey,
            () => GetStringOrDefault(PcsVariable.PCS_TWIN_REGISTRY_URL,
                () => GetDefaultUrl("9042", "registry")));

        /// <inheritdoc/>
        public DiscoveryConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
