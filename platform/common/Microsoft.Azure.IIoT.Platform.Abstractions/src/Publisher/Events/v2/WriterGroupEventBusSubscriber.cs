// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer Group registry change listener
    /// </summary>
    public sealed class WriterGroupEventBusSubscriber : IEventBusConsumer<WriterGroupEventModel> {

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public WriterGroupEventBusSubscriber(IEnumerable<IWriterGroupRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IWriterGroupRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(WriterGroupEventModel eventData) {
            switch (eventData.EventType) {
                case WriterGroupEventType.Added:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupAddedAsync(
                            eventData.Context, eventData.WriterGroup)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case WriterGroupEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupUpdatedAsync(
                            eventData.Context, eventData.WriterGroup)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case WriterGroupEventType.StateChange:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupStateChangeAsync(
                            eventData.Context, eventData.WriterGroup)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case WriterGroupEventType.Activated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupActivatedAsync(
                            eventData.Context, eventData.WriterGroup)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case WriterGroupEventType.Deactivated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupDeactivatedAsync(
                            eventData.Context, eventData.WriterGroup)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
                case WriterGroupEventType.Removed:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnWriterGroupRemovedAsync(
                            eventData.Context, eventData.Id)
                        .ContinueWith(t => Task.CompletedTask))).ConfigureAwait(false);
                    break;
            }
        }

        private readonly List<IWriterGroupRegistryListener> _listeners;
    }
}
