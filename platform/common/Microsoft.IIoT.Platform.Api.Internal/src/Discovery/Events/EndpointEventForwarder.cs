// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Clients {
    using Microsoft.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.IIoT.Platform.Discovery.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Endpoint registry event publisher
    /// </summary>
    public class EndpointEventForwarder<THub> : IEventBusConsumer<EndpointEventModel> {

        /// <inheritdoc/>
        public EndpointEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(EndpointEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.EndpointEventTarget, arguments);
        }
        private readonly ICallbackInvoker _callback;
    }
}
