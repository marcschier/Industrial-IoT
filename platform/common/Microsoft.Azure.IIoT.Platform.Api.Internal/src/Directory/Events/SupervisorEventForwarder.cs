﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Supervisor registry event publisher
    /// </summary>
    public class SupervisorEventForwarder<THub> : IEventHandler<SupervisorEventModel> {

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
