// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Event bus subscriber extensions
    /// </summary>
    public static class EventBusSubscriberEx {

        /// <summary>
        /// Register callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Task<IAsyncDisposable> SubscribeAsync<T>(this IEventBusSubscriber bus,
            Func<T, Task> handler) {
            if (bus is null) {
                throw new ArgumentNullException(nameof(bus));
            }
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            return bus.SubscribeAsync(new DelegateHandler<T>(handler));
        }

        /// <summary>
        /// Helper wrapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DelegateHandler<T> : IEventBusConsumer<T> {

            internal DelegateHandler(Func<T, Task> handler) {
                _handler = handler;
            }

            /// <inheritdoc/>
            public Task HandleAsync(T eventData) {
                return _handler(eventData);
            }

            private readonly Func<T, Task> _handler;
        }
    }
}
