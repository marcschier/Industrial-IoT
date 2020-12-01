// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Service {
    using Microsoft.IIoT.Platform.Discovery.Api;
    using Microsoft.IIoT.Platform.Discovery.Api.Runtime;
    using Microsoft.IIoT.Platform.Vault;
    using Microsoft.IIoT.Platform.Vault.Runtime;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Web service configuration
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<WebHostOptions>,
         IVaultConfig, IDiscoveryConfig {

        /// <inheritdoc/>
        public bool AutoApprove => _vault.AutoApprove;

        /// <inheritdoc/>
        public string ContainerName => "iiot_opc";
        /// <inheritdoc/>
        public string DatabaseName => "iiot_opc";


        /// <inheritdoc/>
        public string DiscoveryServiceUrl => _registry.DiscoveryServiceUrl;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        internal HostingOptions(IConfiguration configuration) :
            base(configuration) {
            _vault = new VaultConfig(configuration);
            _registry = new DiscoveryConfig(configuration);
        }

        /// <inheritdoc/>
        public override void Configure(string name, WebHostOptions options) {
            options.ServicePathBase = GetStringOrDefault(
                PcsVariable.PCS_VAULT_SERVICE_PATH_BASE);
        }

        private readonly IVaultConfig _vault;
        private readonly DiscoveryConfig _registry;
    }
}

