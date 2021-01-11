// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging.Services {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Utils;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System;
    using System.Linq;

    /// <summary>
    /// Simple in memory event bus
    /// </summary>
    public class SimpleEventBus : IEventBusPublisher, IEventBusSubscriber {

        /// <inheritdoc/>
        public async Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var name = typeof(T).GetMoniker();
            await Task.WhenAll(_handlers.Values
                .Where(h => h.Moniker == name)
                .Select(h => h.HandleAsync(message))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<IAsyncDisposable> SubscribeAsync<T>(IEventBusConsumer<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            var handle = new Handle(typeof(T), handler);
            _handlers.TryAdd(handle.Token, handle);
            return Task.FromResult<IAsyncDisposable>(new AsyncDisposable(() => {
                _handlers.TryRemove(handle.Token, out _);
                return Task.CompletedTask;
            }));
        }

        /// <summary>
        /// Encapsulates the handler
        /// </summary>
        private class Handle {

            /// <summary>
            /// Registration
            /// </summary>
            public string Token { get; } = Guid.NewGuid().ToString();

            /// <summary>
            /// Moniker
            /// </summary>
            public string Moniker { get; }

            /// <summary>
            /// Create handler
            /// </summary>
            /// <param name="type"></param>
            /// <param name="handler"></param>
            public Handle(Type type, IHandler handler) {
                _type = type;
                _handler = handler;
                Moniker = type.GetMoniker();
            }

            /// <summary>
            /// Handle message
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="message"></param>
            /// <returns></returns>
            public Task HandleAsync<T>(T message) {
                System.Diagnostics.Debug.Assert(typeof(T) == _type);
                return ((IEventBusConsumer<T>)_handler).HandleAsync(message);
            }

            private readonly Type _type;
            private readonly IHandler _handler;
        }

        private readonly ConcurrentDictionary<string, Handle> _handlers =
            new ConcurrentDictionary<string, Handle>();
    }
}