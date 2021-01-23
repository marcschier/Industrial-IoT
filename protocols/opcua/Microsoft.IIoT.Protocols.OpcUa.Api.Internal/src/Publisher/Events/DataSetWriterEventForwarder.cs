// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Application registry event publisher
    /// </summary>
    public class DataSetWriterEventForwarder<THub> : IEventBusConsumer<DataSetWriterEventModel> {

        /// <inheritdoc/>
        public DataSetWriterEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(DataSetWriterEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.WriterEventTarget, arguments);
        }

        private readonly ICallbackInvoker _callback;
    }
}
