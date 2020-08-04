// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2 {
    using Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// DataSet Writer registry event publisher
    /// </summary>
    public class DataSetWriterEventBusPublisher : IDataSetWriterRegistryListener {

        /// <summary>
        /// Create event publisher
        /// </summary>
        /// <param name="bus"></param>
        public DataSetWriterEventBusPublisher(IEventBus bus) {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterAddedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            return _bus.PublishAsync(Wrap(DataSetWriterEventType.Added, context,
                dataSetWriter.DataSetWriterId, dataSetWriter));
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterStateChangeAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter) {
            return _bus.PublishAsync(Wrap(DataSetWriterEventType.StateChange, context,
                dataSetWriterId, dataSetWriter));
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterUpdatedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, DataSetWriterInfoModel dataSetWriter) {
            return _bus.PublishAsync(Wrap(DataSetWriterEventType.Updated, context,
                dataSetWriterId, dataSetWriter));
        }

        /// <inheritdoc/>
        public Task OnDataSetWriterRemovedAsync(PublisherOperationContextModel context,
            DataSetWriterInfoModel dataSetWriter) {
            return _bus.PublishAsync(Wrap(DataSetWriterEventType.Removed, context,
                dataSetWriter.DataSetWriterId, dataSetWriter));
        }

        /// <summary>
        /// Create data set writer event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        private static DataSetWriterEventModel Wrap(DataSetWriterEventType type,
            PublisherOperationContextModel context, string dataSetWriterId,
            DataSetWriterInfoModel dataSetWriter) {
            return new DataSetWriterEventModel {
                EventType = type,
                Context = context,
                Id = dataSetWriterId,
                DataSetWriter = dataSetWriter
            };
        }

        private readonly IEventBus _bus;
    }
}
