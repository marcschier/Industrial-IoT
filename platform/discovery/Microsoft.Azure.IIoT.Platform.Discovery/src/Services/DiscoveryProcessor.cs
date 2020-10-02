// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Services {
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Processes the discovery results received from edge application discovery
    /// </summary>
    public sealed class DiscoveryProcessor : IDiscoveryResultProcessor {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="gateways"></param>
        /// <param name="applications"></param>
        public DiscoveryProcessor(IGatewayRegistry gateways,
            IApplicationBulkProcessor applications) {
            _gateways = gateways ??
                throw new ArgumentNullException(nameof(gateways));
            _applications = applications ??
                throw new ArgumentNullException(nameof(applications));
        }

        /// <inheritdoc/>
        public async Task ProcessDiscoveryResultsAsync(string discovererId,
            DiscoveryResultModel result, IEnumerable<DiscoveryEventModel> events) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }

            var gatewayId = HubResource.Parse(discovererId, out _, out _);

            if (result == null) {
                throw new ArgumentNullException(nameof(result));
            }
            if (events == null) {
                throw new ArgumentNullException(nameof(events));
            }
            if ((result.RegisterOnly ?? false) && !events.Any()) {
                return;
            }

            var gateway = await _gateways.GetGatewayAsync(gatewayId).ConfigureAwait(false);
            var siteId = gateway?.Gateway?.SiteId ?? gatewayId;

            //
            // Merge in global discovery configuration into the one sent
            // by the discoverer.
            //
            if (result.DiscoveryConfig == null) {
                // Use global discovery configuration
                result.DiscoveryConfig = gateway.Modules?.Discoverer?.DiscoveryConfig;
            }

            // Process discovery events
            await _applications.ProcessDiscoveryEventsAsync(siteId, discovererId,
                gateway.Modules?.Supervisor?.Id, result, events).ConfigureAwait(false);
        }

        private readonly IGatewayRegistry _gateways;
        private readonly IApplicationBulkProcessor _applications;
    }
}
