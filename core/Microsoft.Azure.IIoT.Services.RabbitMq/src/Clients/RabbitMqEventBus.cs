// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Buffers;
    using RabbitMQ.Client;

    /// <summary>
    /// Event bus built on top of rabbitmq
    /// </summary>
    public sealed class RabbitMqEventBus : IEventBus, IDisposable {

        /// <summary>
        /// Create mass transit bus event bus
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public RabbitMqEventBus(IRabbitMqConnection connection,
            IJsonSerializer serializer, ILogger logger) {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _connection = connection ??
                throw new ArgumentNullException(nameof(connection));
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var eventName = typeof(T).GetMoniker();
            try {
                var channel = GetPublisherChannel(eventName);
                var writer = new ArrayBufferWriter<byte>();
                _serializer.Serialize(writer, message);
                var tcs = new TaskCompletionSource<bool>();
                channel.Publish(writer.WrittenMemory, tcs, (t, ex) => {
                    if (ex != null) {
                        t.SetException(ex);
                    }
                    else {
                        _logger.Verbose("----->  {@message} sent...", message);
                        t.SetResult(true);
                    }
                });
                return tcs.Task;
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to publish message {@message}",
                    message);
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _lock.Dispose();
        }

        /// <inheritdoc/>
        public async Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            var eventName = typeof(T).GetMoniker();
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                var tag = Guid.NewGuid().ToString();
                if (!_consumers.TryGetValue(eventName, out var consumer)) {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    consumer = new Consumer<T>(this, eventName);
#pragma warning restore CA2000 // Dispose objects before losing scope
                    _consumers.TryAdd(eventName, consumer);
                }
                consumer.Add(tag, handler);
                return tag;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task UnregisterAsync(string token) {
            if (string.IsNullOrEmpty(token)) {
                throw new ArgumentNullException(nameof(token));
            }
            string eventName = null;
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                var found = false;
                foreach (var consumer in _consumers) {
                    if (consumer.Value.Remove(token, out var handler)) {
                        eventName = consumer.Key;
                    }
                    if (handler != null) {
                        found = true;
                        break; // Found
                    }
                }
                if (!found) {
                    throw new ResourceInvalidStateException("Token not found");
                }

                // Clean up consumer
                if (!string.IsNullOrEmpty(eventName)) {
                    if (_consumers.TryGetValue(eventName, out var consumer)) {
                        consumer.Dispose();
                        _consumers.Remove(eventName);
                    }
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Subscription holder
        /// </summary>
        private class Consumer<T> : IRabbitMqConsumer, ISubscription {

            /// <summary>
            /// Create consumer
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="eventName"></param>
            public Consumer(RabbitMqEventBus outer, string eventName) {
                _outer = outer;
                // Register this consumer on pub/sub connection
                _channel = outer._connection.GetChannel(eventName, this, true);
            }

            /// <inheritdoc/>
            public bool Remove(string token, out IHandler handler) {
                _lock.Wait();
                try {
                    if (_subscriptions.TryGetValue(token, out var found)) {
                        _subscriptions.Remove(token);
                        handler = found;
                        if (_subscriptions.Count == 0) {
                            return true; // Calls dispose
                        }
                    }
                    else {
                        handler = null;
                    }
                    return false;
                }
                finally {
                    _lock.Release();
                }
            }

            /// <inheritdoc/>
            public void Add(string token, IHandler handler) {
                _lock.Wait();
                try {
                    var first = _subscriptions.Count == 0;
                    _subscriptions.Add(token, (IEventHandler<T>)handler);
                }
                finally {
                    _lock.Release();
                }
            }

            /// <inheritdoc/>
            public async Task HandleBasicDeliver(IModel model,
                ulong deliveryTag, bool redelivered, string exchange,
                string routingKey, IBasicProperties properties,
                ReadOnlyMemory<byte> body) {
                var evt = _outer._serializer.Deserialize<T>(body);
                List<IEventHandler<T>> handlers;
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    handlers = _subscriptions.Values.ToList();
                }
                finally {
                    _lock.Release();
                }
                foreach (var handler in handlers) {
                    await handler.HandleAsync(evt).ConfigureAwait(false);
                    _outer._logger.Verbose(
                        "<-----  {@message} received and handled! ", evt);
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                _channel.Dispose();
                _lock.Dispose();
            }

            private readonly RabbitMqEventBus _outer;
            private readonly IRabbitMqChannel _channel;
            private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
            private readonly Dictionary<string, IEventHandler<T>> _subscriptions =
                new Dictionary<string, IEventHandler<T>>();
        }

        /// <summary>
        /// Subscription interface
        /// </summary>
        private interface ISubscription : IDisposable {

            /// <summary>
            /// Remove Handler
            /// </summary>
            /// <param name="token"></param>
            /// <param name="handler"></param>
            /// <returns></returns>
            bool Remove(string token, out IHandler handler);

            /// <summary>
            /// Add handler
            /// </summary>
            /// <param name="token"></param>
            /// <param name="handler"></param>
            void Add(string token, IHandler handler);
        }

        /// <summary>
        /// Get producer channel
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private IRabbitMqChannel GetPublisherChannel(string eventName) {
            // Get channel from channel cache
            return _producers.GetOrAdd(eventName,
                n => _connection.GetChannel(n, fanout: true));
        }

        private readonly Dictionary<string, ISubscription> _consumers =
            new Dictionary<string, ISubscription>();
        private readonly ConcurrentDictionary<string, IRabbitMqChannel> _producers =
            new ConcurrentDictionary<string, IRabbitMqChannel>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly IRabbitMqConnection _connection;
    }
}