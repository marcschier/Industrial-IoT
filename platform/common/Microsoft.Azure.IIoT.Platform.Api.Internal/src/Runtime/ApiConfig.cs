// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Api.Runtime {
    using Microsoft.Azure.IIoT.Platform.Publisher.Api;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.History.Api;
    using Microsoft.Azure.IIoT.Platform.History.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Twin.Api;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Vault.Api;
    using Microsoft.Azure.IIoT.Platform.Vault.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Events.Api.Runtime;
    using Microsoft.Azure.IIoT.Platform.Events.Api;
    using Microsoft.Azure.IIoT.Messaging.SignalR;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Complete api configuration
    /// </summary>
    public class ApiConfig : DiagnosticsConfig, ITwinConfig, IRegistryConfig,
        IVaultConfig, IHistoryConfig, IPublisherConfig, IEventsConfig,
        ISignalRClientConfig {

        /// <inheritdoc/>
        public string OpcUaTwinServiceUrl => _twin.OpcUaTwinServiceUrl;

        /// <inheritdoc/>
        public string OpcUaRegistryServiceUrl => _registry.OpcUaRegistryServiceUrl;

        /// <inheritdoc/>
        public string OpcUaVaultServiceUrl => _vault.OpcUaVaultServiceUrl;

        /// <inheritdoc/>
        public string OpcUaHistoryServiceUrl => _history.OpcUaHistoryServiceUrl;

        /// <inheritdoc/>
        public string OpcUaPublisherServiceUrl => _publisher.OpcUaPublisherServiceUrl;

        /// <inheritdoc/>
        public string OpcUaEventsServiceUrl => _events.OpcUaEventsServiceUrl;

        /// <inheritdoc/>
        public bool UseMessagePackProtocol => _events.UseMessagePackProtocol;

        /// <inheritdoc/>
        public ApiConfig(IConfiguration configuration) :
            base(configuration) {
            _twin = new TwinConfig(configuration);
            _registry = new RegistryConfig(configuration);
            _vault = new VaultConfig(configuration);
            _history = new HistoryConfig(configuration);
            _publisher = new PublisherConfig(configuration);
            _events = new EventsConfig(configuration);
        }

        private readonly TwinConfig _twin;
        private readonly RegistryConfig _registry;
        private readonly VaultConfig _vault;
        private readonly HistoryConfig _history;
        private readonly PublisherConfig _publisher;
        private readonly EventsConfig _events;
    }
}
