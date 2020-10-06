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
    /// Discoverer registry event publisher
    /// </summary>
    public class DiscovererEventForwarder<THub> : IEventHandler<DiscovererEventModel> {

        /// <inheritdoc/>
        public DiscovererEventForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleAsync(DiscovererEventModel eventData) {
            var arguments = new object[] { eventData.ToApiModel() };
            return _callback.BroadcastAsync(
                EventTargets.DiscovererEventTarget, arguments);
        }

        private readonly ICallbackInvoker _callback;
    }
}
