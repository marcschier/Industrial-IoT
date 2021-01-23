// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Api.Runtime {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Events.Api.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Events.Api;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Complete api configuration
    /// </summary>
    public class ApiConfig : ConfigureOptionBase, ITwinConfig, IDiscoveryConfig,
        IPublisherConfig, IEventsConfig {

        /// <inheritdoc/>
        public string OpcUaTwinServiceUrl => _twin.OpcUaTwinServiceUrl;

        /// <inheritdoc/>
        public string DiscoveryServiceUrl => _registry.DiscoveryServiceUrl;

        /// <inheritdoc/>
        public string OpcUaPublisherServiceUrl => _publisher.OpcUaPublisherServiceUrl;

        /// <inheritdoc/>
        public string OpcUaEventsServiceUrl => _events.OpcUaEventsServiceUrl;

        /// <inheritdoc/>
        public ApiConfig(IConfiguration configuration) :
            base(configuration) {
            _twin = new TwinConfig(configuration);
            _registry = new DiscoveryConfig(configuration);
            _publisher = new PublisherConfig(configuration);
            _events = new EventsConfig(configuration);
        }

        private readonly TwinConfig _twin;
        private readonly DiscoveryConfig _registry;
        private readonly PublisherConfig _publisher;
        private readonly EventsConfig _events;
    }
}
