// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Clients {
    using Microsoft.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Rpc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Endpoint registry event publisher
    /// </summary>
    public class DiscoveryProgressForwarder<THub> : IEventBusConsumer<DiscoveryProgressModel> {

        /// <inheritdoc/>
        public DiscoveryProgressForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public async Task HandleAsync(DiscoveryProgressModel eventData) {
            if (eventData.TimeStamp + TimeSpan.FromSeconds(10) < DateTime.UtcNow) {
                // Do not forward stale events - todo make configurable / add metric
                return;
            }
            var requestId = eventData.Request?.Id;
            var arguments = new object[] { eventData.ToApiModel() };
            if (!string.IsNullOrEmpty(requestId)) {
                // Send to user
                await _callback.MulticastAsync(requestId,
                    EventTargets.DiscoveryProgressTarget, arguments).ConfigureAwait(false);
            }
            if (!string.IsNullOrEmpty(eventData.DiscovererId)) {
                // Send to discovery listeners
                await _callback.MulticastAsync(eventData.DiscovererId,
                    EventTargets.DiscoveryProgressTarget, arguments).ConfigureAwait(false);
            }
        }

        private readonly ICallbackInvoker _callback;
    }
}
