// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.Azure.ServiceBus;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service bus queue client
    /// </summary>
    public sealed class ServiceBusQueueClient : IEventPublisherClient, IEventClient {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="factory"></param>
        public ServiceBusQueueClient(IServiceBusClientFactory factory) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        public async Task PublishAsync(string target, byte[] payload,
            IEventProperties properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var msg = new Message(payload) {
                MessageId = Interlocked.Increment(ref _id).ToString(),
                PartitionKey = GetKey(target, null, partitionKey)
            };
            if (properties != null) {
                foreach (var prop in properties) {
                    msg.UserProperties.Add(prop.Key, prop.Value);
                }
                if (properties.TryGetValue(EventProperties.ContentType,
                    out var contentType)) {
                    msg.ContentType = contentType;
                }
            }
            var queue = target.Split('/')[0];
            var client = await _factory.CreateOrGetGetQueueClientAsync(queue).ConfigureAwait(false);
            await client.SendAsync(msg).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task PublishAsync(string target, IEnumerable<byte[]> batch,
            IEventProperties properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            foreach (var payload in batch) {
                await PublishAsync(target, payload, properties, partitionKey,
                    ct).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Publish<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IEventProperties properties,
            string partitionKey) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            var t = PublishAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait(); // Wait to create proper send sequence.
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var queue = target.Split('/')[0];
            var client = await _factory.CreateOrGetGetQueueClientAsync(queue).ConfigureAwait(false);
            await client.SendAsync(CreateMessage(
                target, payload, contentType, eventSchema, contentEncoding)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var queue = target.Split('/')[0];
            var messages = batch
                .Select(payload => CreateMessage(target, payload,
                    contentType, eventSchema, contentEncoding));
            //
            // Calculating the wire size is too much effort and sending batch will
            // pull the entire amqp message into a batch object which is limited to
            // 256 kb in size.  Hence send all messages individually.
            //
            foreach (var message in messages) {
                var client = await _factory.CreateOrGetGetQueueClientAsync(queue).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token,
            Action<T, Exception> complete) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            var t = SendEventAsync(target, payload, contentType, eventSchema,
                contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait(); // Wait to create proper send sequence.
        }

        /// <summary>
        /// Helper to create event from buffer and content type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private Message CreateMessage(string target, byte[] data,
            string contentType, string eventSchema, string contentEncoding) {
            var ev = new Message(data) {
                MessageId = Interlocked.Increment(ref _id).ToString(),
                ContentType = contentType,
                PartitionKey = GetKey(target, eventSchema, null)
            };
            ev.UserProperties.Add(EventProperties.Target, target);
            if (!string.IsNullOrEmpty(contentType)) {
                ev.UserProperties.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                ev.UserProperties.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                ev.UserProperties.Add(EventProperties.EventSchema, eventSchema);
            }
            return ev;
        }

        /// <summary>
        /// Get partition key
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="target"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        private static string GetKey(string target, string schema, string partitionKey) {
            var key = string.IsNullOrEmpty(partitionKey) ? target : partitionKey;
            if (!string.IsNullOrEmpty(schema)) {
                key += schema;
            }
            return key.ToLowerInvariant().GetHashCode().ToString();
        }

        private readonly IServiceBusClientFactory _factory;
        private volatile int _id;
    }
}
