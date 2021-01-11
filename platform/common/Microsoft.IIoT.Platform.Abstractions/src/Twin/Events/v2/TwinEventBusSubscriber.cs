// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Events.v2 {
    using Microsoft.IIoT.Platform.Twin.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin registry change listener
    /// </summary>
    public class TwinEventBusSubscriber : IEventBusConsumer<TwinEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public TwinEventBusSubscriber(IEnumerable<ITwinRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<ITwinRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(TwinEventModel eventData) {
            switch (eventData.EventType) {
                case TwinEventType.Activated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnTwinActivatedAsync(
                            eventData.Context, eventData.Twin)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case TwinEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnTwinUpdatedAsync(
                            eventData.Context, eventData.Twin)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case TwinEventType.Deactivated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnTwinDeactivatedAsync(
                            eventData.Context, eventData.Twin)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<ITwinRegistryListener> _listeners;
    }
}
