﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Events.v2 {
    using Microsoft.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Gateway registry change listener
    /// </summary>
    public class GatewayEventBusSubscriber : IEventBusConsumer<GatewayEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public GatewayEventBusSubscriber(IEnumerable<IGatewayRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IGatewayRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(GatewayEventModel eventData) {
            switch (eventData.EventType) {
                case GatewayEventType.New:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnGatewayNewAsync(
                            eventData.Context, eventData.Gateway)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case GatewayEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnGatewayUpdatedAsync(
                            eventData.Context, eventData.Gateway)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case GatewayEventType.Deleted:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnGatewayDeletedAsync(
                            eventData.Context, eventData.Id)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<IGatewayRegistryListener> _listeners;
    }
}
