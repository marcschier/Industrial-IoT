// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Threading;
    using System.Buffers;
    using RabbitMQ.Client;

    /// <summary>
    /// Event bus built on top of rabbitmq
    /// </summary>
    public class RabbitMqEventBus : IEventBus, IDisposable {

        /// <summary>
        /// Create mass transit bus event bus
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public RabbitMqEventBus(IRabbitMqConnection connection, IJsonSerializer serializer,
            ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var eventName = typeof(T).GetMoniker();
            try {
                var topic = _connection.GetChannel(eventName);
                var properties = topic.CreateBasicProperties();
                var writer = new ArrayBufferWriter<byte>();
                _serializer.Serialize(writer, message);
                topic.BasicPublish("", eventName, false, properties, writer.WrittenMemory);
                _logger.Verbose("----->  {@message} sent...", message);
                return Task.CompletedTask;
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to publish message {@message}", message);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task CloseAsync() {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() {
        }

        /// <inheritdoc/>
        public async Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            var eventName = typeof(T).GetMoniker();
            await _lock.WaitAsync();
            try {
                var token = Guid.NewGuid().ToString();
                if (!_consumers.TryGetValue(eventName, out var consumer)) {
                    consumer = new Consumer<T>(token, this);
                    _consumers.TryAdd(eventName, consumer);
                }
                consumer.Register(token, handler);
                return token;
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
            await _lock.WaitAsync();
            try {
                foreach (var consumer in _consumers) {
                    if (consumer.Value.Unregister(token, out var handler)) {
                        eventName = consumer.Key;
                    }
                    if (handler != null) {
                        break; // Found
                    }
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
        private class Consumer<T> : AsyncDefaultBasicConsumer, IConsumer {

            /// <summary>
            /// Create consumer
            /// </summary>
            /// <param name="token"></param>
            /// <param name="outer"></param>
            public Consumer(string token, RabbitMqEventBus outer) :
                base(outer._connection.GetChannel(typeof(T).GetMoniker())) {
                _outer = outer;
                _consumerTag = ""; // TODO
            }

            /// <inheritdoc/>
            public bool Unregister(string token, out IHandler handler) {
                _lock.Wait();
                try {
                    if (_subscriptions.TryGetValue(token, out var found)) {
                        _subscriptions.Remove(token);
                        handler = found;
                        if (_subscriptions.Count == 0) {
                            Model.BasicCancel(_consumerTag); // Stop consume
                            return true;
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
            public void Register(string token, IHandler handler) {
                _lock.Wait();
                try {
                    var first = _subscriptions.Count == 0;
                    _subscriptions.Add(token, (IEventHandler<T>)handler);
                    if (first) {
                        // Start to consume
                        Model.BasicConsume(typeof(T).GetMoniker(), true, this);
                    }
                }
                finally {
                    _lock.Release();
                }
            }

            /// <inheritdoc/>
            public override async Task HandleBasicDeliver(string consumerTag,
                ulong deliveryTag, bool redelivered, string exchange,
                string routingKey, IBasicProperties properties,
                ReadOnlyMemory<byte> body) {

                if (consumerTag != _consumerTag) {
                    return;
                }
                var evt = _outer._serializer.Deserialize<T>(body);
                List<IEventHandler<T>> handlers;
                await _lock.WaitAsync();
                try {
                    handlers = _subscriptions.Values.ToList();
                    _outer._logger.Verbose("<-----  {@message} received and handled! ", evt);
                }
                finally {
                    _lock.Release();
                }
                foreach (var handler in handlers) {
                    await handler.HandleAsync(evt);
                }
            }

            /// <inheritdoc/>
            public override Task HandleModelShutdown(object model, ShutdownEventArgs reason) {
                return base.HandleModelShutdown(model, reason);
            }

            /// <inheritdoc/>
            public void Dispose() {
                Model.Dispose();
            }

            private readonly string _consumerTag;
            private readonly RabbitMqEventBus _outer;
            private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
            private readonly Dictionary<string, IEventHandler<T>> _subscriptions =
                new Dictionary<string, IEventHandler<T>>();
        }

        /// <summary>
        /// Generic consumer
        /// </summary>
        private interface IConsumer : IDisposable {

            /// <summary>
            /// Remove
            /// </summary>
            /// <param name="token"></param>
            /// <param name="handler"></param>
            /// <returns></returns>
            bool Unregister(string token, out IHandler handler);

            /// <summary>
            /// Add handler
            /// </summary>
            /// <param name="token"></param>
            /// <param name="handler"></param>
            void Register(string token, IHandler handler);
        }

        private readonly Dictionary<string, IConsumer> _consumers =
            new Dictionary<string, IConsumer>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly IRabbitMqConnection _connection;
    }
}