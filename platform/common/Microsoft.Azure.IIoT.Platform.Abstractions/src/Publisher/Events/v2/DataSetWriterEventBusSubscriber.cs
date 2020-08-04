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
    public class DataSetWriterEventBusSubscriber : IEventHandler<DataSetWriterEventModel>{

        /// <summary>
        /// Create event subscriber
        /// </summary>
        /// <param name="listeners"></param>
        public DataSetWriterEventBusSubscriber(IEnumerable<IDataSetWriterRegistryListener> listeners) {
            _listeners = listeners?.ToList() ?? new List<IDataSetWriterRegistryListener>();
        }

        /// <inheritdoc/>
        public async Task HandleAsync(DataSetWriterEventModel eventData) {
            switch (eventData.EventType) {
                case DataSetWriterEventType.Added:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDataSetWriterAddedAsync(
                            eventData.Context, eventData.DataSetWriter)
                        .ContinueWith(t => Task.CompletedTask)));
                    break;
                case DataSetWriterEventType.Updated:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDataSetWriterUpdatedAsync(
                            eventData.Context, eventData.Id, eventData.DataSetWriter)
                        .ContinueWith(t => Task.CompletedTask)));
                    break;
                case DataSetWriterEventType.StateChange:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDataSetWriterStateChangeAsync(
                            eventData.Context, eventData.Id, eventData.DataSetWriter)
                        .ContinueWith(t => Task.CompletedTask)));
                    break;
                case DataSetWriterEventType.Removed:
                    await Task.WhenAll(_listeners
                        .Select(l => l.OnDataSetWriterRemovedAsync(
                            eventData.Context, eventData.DataSetWriter)
                        .ContinueWith(t => Task.CompletedTask)));
                    break;
            }
        }

        private readonly List<IDataSetWriterRegistryListener> _listeners;
    }
}
