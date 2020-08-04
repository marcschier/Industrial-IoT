// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Subscriber;
    using Microsoft.Azure.IIoT.Platform.Subscriber.Models;
    using Microsoft.Azure.IIoT.Rpc;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset message publishing
    /// </summary>
    public sealed class DataSetWriterMessagePublisher<THub> : ISubscriberMessageProcessor,
        IDisposable {

        /// <summary>
        /// Create publisher
        /// </summary>
        /// <param name="callback"></param>
        public DataSetWriterMessagePublisher(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public Task HandleSampleAsync(MonitoredItemMessageModel sample) {
            // TODO: Should we convert to dataset message like below?
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task HandleMessageAsync(DataSetMessageModel message) {
            if (string.IsNullOrEmpty(message.DataSetWriterId)) {
                return;
            }
            // Send to dataset listeners
            await _callback.MulticastAsync(message.DataSetWriterId,
                EventTargets.DataSetMessageTarget, new object[] { message });
        }

        /// <inheritdoc/>
        public void Dispose() {
            _callback.Dispose();
        }

        private readonly ICallbackInvoker _callback;
    }
}
