﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api.Clients {
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using Microsoft.IIoT.Platform.Registry.Events.v2.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Supervisor registry event publisher
    /// </summary>
    public class SupervisorEventForwarder<THub> : IEventBusConsumer<SupervisorEventModel> {

        /// <inheritdoc/>
        public SupervisorEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(SupervisorEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.SupervisorEventTarget, arguments);
        }
        private readonly ICallbackInvoker _callback;
    }
}
