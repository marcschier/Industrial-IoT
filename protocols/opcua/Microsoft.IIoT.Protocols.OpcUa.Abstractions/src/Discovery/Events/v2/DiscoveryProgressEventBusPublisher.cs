// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Events.v2 {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Discoverer progress processor
    /// </summary>
    public class DiscoveryProgressEventBusPublisher : IDiscovererProgressProcessor {

        /// <summary>
        /// Create event discoverer
        /// </summary>
        /// <param name="bus"></param>
        public DiscoveryProgressEventBusPublisher(IEventBusPublisher bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnDiscoveryProgressAsync(DiscoveryProgressModel message) {
            if (message.TimeStamp + TimeSpan.FromSeconds(10) < DateTime.UtcNow) {
                // Do not forward stale events - todo make configurable / add metric
                return Task.CompletedTask;
            }
            return _bus.PublishAsync(message);
        }

        private readonly IEventBusPublisher _bus;
    }
}
