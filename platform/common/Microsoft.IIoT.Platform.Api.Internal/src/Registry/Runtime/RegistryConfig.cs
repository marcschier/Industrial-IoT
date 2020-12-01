// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api.Runtime {
    using Microsoft.IIoT.Api.Runtime;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration - wraps a configuration root
    /// </summary>
    public class RegistryConfig : ApiConfigBase, IRegistryConfig {

        /// <summary>
        /// Registry configuration
        /// </summary>
        private const string kDirectoryServiceUrlKey = "RegistryServiceUrl";

        /// <summary>Directory endpoint url</summary>
        public string RegistryServiceUrl => GetStringOrDefault(
            kDirectoryServiceUrlKey,
                GetStringOrDefault(PcsVariable.PCS_REGISTRY_SERVICE_URL,
                GetDefaultUrl("9043", "directory")));

        /// <inheritdoc/>
        public RegistryConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
