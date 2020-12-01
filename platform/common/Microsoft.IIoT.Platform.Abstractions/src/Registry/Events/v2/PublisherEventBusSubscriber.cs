﻿// ------------------------------------------------------------
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
    /// Publisher registry change listener
    /// </summary>
    public class PublisherEventBusSubscriber : IEventBusConsumer<PublisherEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public PublisherEventBusSubscriber(IEnumerable<IPublisherRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IPublisherRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(PublisherEventModel eventData) {
            switch (eventData.EventType) {
                case PublisherEventType.New:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnPublisherNewAsync(
                            eventData.Context, eventData.Publisher)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case PublisherEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnPublisherUpdatedAsync(
                            eventData.Context, eventData.Publisher)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case PublisherEventType.Deleted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnPublisherDeletedAsync(
                            eventData.Context, eventData.Id)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<IPublisherRegistryListener> _listeners;
    }
}
