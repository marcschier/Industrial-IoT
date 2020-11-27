﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Event subscriber extensions
    /// </summary>
    public static class EventSubscriberClientEx {

        /// <summary>
        /// Register callback
        /// </summary>
        /// <param name="target"></param>
        /// <param name="client"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Task<IAsyncDisposable> SubscribeAsync(this IEventSubscriberClient client,
            string target, Func<byte[], IDictionary<string, string>, Task> handler) {
            if (client is null) {
                throw new ArgumentNullException(nameof(client));
            }
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            return client.SubscribeAsync(target, new DelegateHandler(handler));
        }

        /// <summary>
        /// Helper wrapper
        /// </summary>
        private class DelegateHandler : IEventConsumer {

            internal DelegateHandler(Func<byte[], IDictionary<string, string>, Task> handler) {
                _handler = handler;
            }

            /// <inheritdoc/>
            public Task HandleAsync(byte[] eventData,
                IDictionary<string, string> properties, Func<Task> checkpoint) {
                return _handler(eventData, properties);
            }

            public Task OnBatchCompleteAsync() {
                return Task.CompletedTask;
            }

            private readonly Func<byte[], IDictionary<string, string>, Task> _handler;
        }
    }
}
