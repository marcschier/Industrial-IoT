// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Twin event publisher
    /// </summary>
    public class TwinEventForwarder<THub> : IEventBusConsumer<TwinEventModel> {

        /// <inheritdoc/>
        public TwinEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(TwinEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.TwinEventTarget, arguments);
        }
        private readonly ICallbackInvoker _callback;
    }
}
