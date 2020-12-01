// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api.Clients {
    using Microsoft.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.IIoT.Platform.Publisher;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Rpc;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Data set message progress publishing
    /// </summary>
    public sealed class DataSetWriterMessageForwarder<THub> : IDataSetWriterMessageProcessor,
        IDisposable {

        /// <summary>
        /// Create publisher
        /// </summary>
        /// <param name="callback"></param>
        public DataSetWriterMessageForwarder(ICallbackInvokerT<THub> callback) {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <inheritdoc/>
        public async Task HandleMessageAsync(PublishedDataSetItemMessageModel message) {
            var arguments = new object[] { message.ToApiModel() };
            if (!string.IsNullOrEmpty(message.VariableId)) {
                await _callback.MulticastAsync(message.DataSetWriterId + "_" + message.VariableId,
                    EventTargets.DataSetVariableMessageTarget, arguments).ConfigureAwait(false);
            }
            else {
                await _callback.MulticastAsync(message.DataSetWriterId + "_event",
                    EventTargets.DataSetEventMessageTarget, arguments).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _callback.Dispose();
        }

        private readonly ICallbackInvoker _callback;
    }
}
