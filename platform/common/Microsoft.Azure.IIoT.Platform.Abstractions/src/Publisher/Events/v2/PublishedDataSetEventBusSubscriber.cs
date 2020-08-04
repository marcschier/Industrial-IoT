// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// DataSet Writer registry change listener
    /// </summary>
    public class PublishedDataSetEventBusSubscriber : IEventHandler<PublishedDataSetItemEventModel>{

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public PublishedDataSetEventBusSubscriber(IEnumerable<IPublishedDataSetListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IPublishedDataSetListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(PublishedDataSetItemEventModel eventData) {
            if (eventData.DataSetVariable != null) {
                switch (eventData.EventType) {
                    case PublishedDataSetItemEventType.Added:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetVariableAddedAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.DataSetVariable)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.Updated:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetVariableUpdatedAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.DataSetVariable)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.StateChange:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetVariableStateChangeAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.DataSetVariable)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.Removed:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetVariableRemovedAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.VariableId)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                }
            }
            else {
                switch (eventData.EventType) {
                    case PublishedDataSetItemEventType.Added:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetEventsAddedAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.EventDataSet)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.Updated:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetEventsUpdatedAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.EventDataSet)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.StateChange:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetEventsStateChangeAsync(
                                eventData.Context, eventData.DataSetWriterId, eventData.EventDataSet)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                    case PublishedDataSetItemEventType.Removed:
                        await Task.WhenAll(_listeners
                            .Select(l => l.OnPublishedDataSetEventsRemovedAsync(
                                eventData.Context, eventData.DataSetWriterId)
                            .ContinueWith(t => Task.CompletedTask)));
                        break;
                }
            }
        }

        private readonly List<IPublishedDataSetListener> _listeners;
    }
}
