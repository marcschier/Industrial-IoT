// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Clients {
    using Microsoft.Azure.IIoT.Extensions.Orleans;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System;

    /// <summary>
    /// Orleans event bus client
    /// </summary>
    public class OrleansEventBusClient : IEventBus {

        /// <summary>
        /// Create event bus client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="processor"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public OrleansEventBusClient(IOrleansGrainClient client, IJsonSerializer serializer,
            ITaskProcessor processor, IOptionsSnapshot<OrleansBusOptions> options, 
            ILogger logger) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refs = new ConcurrentDictionary<string, Subscription>();
        }

        /// <inheritdoc/>
        public async Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var topicName = (_options.Value.Prefix ?? "") + typeof(T).GetMoniker();
            var topic = _client.Grains.GetGrain<IOrleansTopic>(topicName);

            var buffer = _serializer.SerializeToBytes(message).ToArray();
            await topic.PublishAsync(buffer).ConfigureAwait(true);
            _logger.LogTrace("Published message through topic {topic}.",
                topicName);
        }

        /// <inheritdoc/>
        public async Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            var topicName = (_options.Value.Prefix ?? "") + typeof(T).GetMoniker();
            var topic = _client.Grains.GetGrain<IOrleansTopic>(topicName);

            var subscription = new Subscription(topic, topicName, buffer => 
                Deliver(handler, buffer));
            var reference = await _client.Grains.CreateObjectReference<IOrleansSubscription>(
                subscription).ConfigureAwait(true);
            subscription.Reference = reference;
            var token = Guid.NewGuid().ToString();
            _refs.TryAdd(token, subscription);

            await topic.SubscribeAsync(reference).ConfigureAwait(true);
            _logger.LogInformation("Registered subscriber to topic {topic}.",
                topicName);
            return token;
        }

        /// <inheritdoc/>
        public async Task UnregisterAsync(string token) {
            if (string.IsNullOrEmpty(token)) {
                throw new ArgumentNullException(nameof(token));
            }
            if (!_refs.TryRemove(token, out var subscription)) {
                throw new ResourceInvalidStateException(nameof(token));
            }

            var topicName = subscription.TopicName;
            var topic = _client.Grains.GetGrain<IOrleansTopic>(topicName);

            await topic.UnsubscribeAsync(subscription.Reference).ConfigureAwait(true);

            await _client.Grains.DeleteObjectReference<IOrleansSubscription>(
                subscription.Reference).ConfigureAwait(true);
            _logger.LogInformation("Unregistered subscriber from topic {topic}.",
                topicName);
        }

        /// <summary>
        /// Callback to handle message delivery
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="buffer"></param>
        protected virtual void Deliver<T>(IEventHandler<T> handler, byte[] buffer) {
            try {
                var message = (T)_serializer.Deserialize(buffer, typeof(T));
                _processor.TrySchedule(() => handler.HandleAsync(message));
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to handle message");
            }
        }

        /// <summary>
        /// Encapsulates the handler
        /// </summary>
        internal sealed class Subscription : IOrleansSubscription {

            /// <summary>
            /// Topic subscribed to - hold to reference to not GC while subscribed
            /// </summary>
            public IOrleansTopic Topic { get; }

            /// <summary>
            /// Reference
            /// </summary>
            public IOrleansSubscription Reference { get; internal set; }

            /// <summary>
            /// Topic name
            /// </summary>
            public string TopicName { get; }

            /// <summary>
            /// Create handler
            /// </summary>
            /// <param name="topic"></param>
            /// <param name="topicName"></param>
            /// <param name="handler"></param>
            public Subscription(IOrleansTopic topic, string topicName, 
                Action<byte[]> handler) {
                Topic = topic;
                TopicName = topicName;
                _handler = handler;
            }

            /// <inheritdoc/>
            public void Consume(byte[] buffer) {
                _handler(buffer);
            }

            private readonly Action<byte[]> _handler;
        }

        private readonly IOptionsSnapshot<OrleansBusOptions> _options;
        private readonly ConcurrentDictionary<string, Subscription> _refs;
        private readonly IOrleansGrainClient _client;
        private readonly IJsonSerializer _serializer;
        private readonly ITaskProcessor _processor;
        private readonly ILogger _logger;
    }
}