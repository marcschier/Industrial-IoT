// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Events.v2 {
    using Microsoft.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.IIoT.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Discoverer change listener
    /// </summary>
    public sealed class DiscovererEventBusSubscriber : IEventBusConsumer<DiscovererEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public DiscovererEventBusSubscriber(
            IEnumerable<IDiscovererRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IDiscovererRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(DiscovererEventModel eventData) {
            switch (eventData.EventType) {
                case DiscovererEventType.New:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDiscovererNewAsync(
                            eventData.Context, eventData.Discoverer)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case DiscovererEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDiscovererUpdatedAsync(
                            eventData.Context, eventData.Discoverer)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case DiscovererEventType.Deleted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDiscovererDeletedAsync(
                            eventData.Context, eventData.Id)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<IDiscovererRegistryListener> _listeners;
    }
}
