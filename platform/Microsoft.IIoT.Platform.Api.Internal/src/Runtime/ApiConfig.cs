// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Api.Runtime {
    using Microsoft.IIoT.Platform.Vault.Api;
    using Microsoft.IIoT.Platform.Vault.Api.Runtime;
    using Microsoft.IIoT.Platform.Events.Api.Runtime;
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Platform.Registry.Api.Runtime;
    using Microsoft.IIoT.Platform.Registry.Api;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Complete api configuration
    /// </summary>
    public class ApiConfig : ConfigureOptionBase, IRegistryConfig,
        IVaultConfig, IEventsConfig {

        /// <inheritdoc/>
        public string OpcUaVaultServiceUrl => _vault.OpcUaVaultServiceUrl;

        /// <inheritdoc/>
        public string OpcUaEventsServiceUrl => _events.OpcUaEventsServiceUrl;

        /// <inheritdoc/>
        public string RegistryServiceUrl => _registry.RegistryServiceUrl;

        /// <inheritdoc/>
        public ApiConfig(IConfiguration configuration) :
            base(configuration) {
            _registry = new RegistryConfig(configuration);
            _vault = new VaultConfig(configuration);
            _events = new EventsConfig(configuration);
        }

        private readonly RegistryConfig _registry;
        private readonly VaultConfig _vault;
        private readonly EventsConfig _events;
    }
}
