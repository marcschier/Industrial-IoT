// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Application registry event publisher
    /// </summary>
    public class ApplicationEventForwarder<THub> : IEventBusConsumer<ApplicationEventModel> {

        /// <inheritdoc/>
        public ApplicationEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(ApplicationEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.ApplicationEventTarget, arguments);
        }

        private readonly ICallbackInvoker _callback;
    }
}
