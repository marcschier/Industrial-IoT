// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Api.Runtime {
    using Microsoft.IIoT.Platform.Publisher.Api;
    using Microsoft.IIoT.Platform.Publisher.Api.Runtime;
    using Microsoft.IIoT.Platform.Twin.Api;
    using Microsoft.IIoT.Platform.Twin.Api.Runtime;
    using Microsoft.IIoT.Platform.Discovery.Api;
    using Microsoft.IIoT.Platform.Discovery.Api.Runtime;
    using Microsoft.IIoT.Platform.Vault.Api;
    using Microsoft.IIoT.Platform.Vault.Api.Runtime;
    using Microsoft.IIoT.Platform.Events.Api.Runtime;
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Complete api configuration
    /// </summary>
    public class ApiConfig : ConfigureOptionBase, ITwinConfig, IDiscoveryConfig,
        IVaultConfig, IPublisherConfig, IEventsConfig {

        /// <inheritdoc/>
        public string OpcUaTwinServiceUrl => _twin.OpcUaTwinServiceUrl;

        /// <inheritdoc/>
        public string DiscoveryServiceUrl => _registry.DiscoveryServiceUrl;

        /// <inheritdoc/>
        public string OpcUaVaultServiceUrl => _vault.OpcUaVaultServiceUrl;

        /// <inheritdoc/>
        public string OpcUaPublisherServiceUrl => _publisher.OpcUaPublisherServiceUrl;

        /// <inheritdoc/>
        public string OpcUaEventsServiceUrl => _events.OpcUaEventsServiceUrl;

        /// <inheritdoc/>
        public ApiConfig(IConfiguration configuration) :
            base(configuration) {
            _twin = new TwinConfig(configuration);
            _registry = new DiscoveryConfig(configuration);
            _vault = new VaultConfig(configuration);
            _publisher = new PublisherConfig(configuration);
            _events = new EventsConfig(configuration);
        }

        private readonly TwinConfig _twin;
        private readonly DiscoveryConfig _registry;
        private readonly VaultConfig _vault;
        private readonly PublisherConfig _publisher;
        private readonly EventsConfig _events;
    }
}
