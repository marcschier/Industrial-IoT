// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Published DataSet item registry event publisher
    /// </summary>
    public class PublishedDataSetEventBusPublisher : IPublishedDataSetListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public PublishedDataSetEventBusPublisher(IEventBus bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetVariableAddedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Added, context,
                dataSetWriterId, dataSetVariable.Id, dataSetVariable));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetVariableUpdatedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Updated, context,
                dataSetWriterId, dataSetVariable.Id, dataSetVariable));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetVariableStateChangeAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.StateChange, context,
                dataSetWriterId, dataSetVariable.Id, dataSetVariable));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetVariableRemovedAsync(OperationContextModel context,
            string dataSetWriterId, string variableId) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Removed, context,
                dataSetWriterId, variableId, null));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetEventsAddedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Added, context,
                dataSetWriterId, eventDataSet));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetEventsUpdatedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Updated, context,
                dataSetWriterId, eventDataSet));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetEventsStateChangeAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.StateChange, context,
                dataSetWriterId, eventDataSet));
        }

        /// <inheritdoc/>
        public Task OnPublishedDataSetEventsRemovedAsync(OperationContextModel context,
            string dataSetWriterId) {
            return _bus.PublishAsync(Wrap(PublishedDataSetItemEventType.Removed, context,
                dataSetWriterId, null));
        }

        /// <summary>
        /// Create variable event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariableId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        private static PublishedDataSetItemEventModel Wrap(PublishedDataSetItemEventType type,
            OperationContextModel context, string dataSetWriterId, string dataSetVariableId,
            PublishedDataSetVariableModel dataSetVariable) {
            return new PublishedDataSetItemEventModel {
                EventType = type,
                Context = context,
                VariableId = dataSetVariableId,
                DataSetWriterId = dataSetWriterId,
                DataSetVariable = dataSetVariable,
                EventDataSet = null
            };
        }

        /// <summary>
        /// Create event definition event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetEvent"></param>
        /// <returns></returns>
        private static PublishedDataSetItemEventModel Wrap(PublishedDataSetItemEventType type,
            OperationContextModel context, string dataSetWriterId,
            PublishedDataSetEventsModel dataSetEvent) {
            return new PublishedDataSetItemEventModel {
                EventType = type,
                Context = context,
                DataSetWriterId = dataSetWriterId,
                VariableId = null,
                DataSetVariable = null,
                EventDataSet = dataSetEvent
            };
        }

        private readonly IEventBus _bus;
    }
}
