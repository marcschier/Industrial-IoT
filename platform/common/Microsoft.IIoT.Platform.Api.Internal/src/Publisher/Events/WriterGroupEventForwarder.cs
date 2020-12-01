﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api.Clients {
    using Microsoft.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.IIoT.Platform.Publisher.Events.v2.Models;
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Application registry event publisher
    /// </summary>
    public class WriterGroupEventForwarder<THub> : IEventBusConsumer<WriterGroupEventModel> {

        /// <inheritdoc/>
        public WriterGroupEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(WriterGroupEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.GroupEventTarget, arguments);
        }

        private readonly ICallbackInvoker _callback;
    }
}
