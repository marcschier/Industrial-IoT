// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using RabbitMQ.Client;

    /// <summary>
    /// RabbitMq queue client
    /// </summary>
    public sealed class RabbitMqQueueClient : IEventQueueClient, IEventClient {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="connection"></param>
        public RabbitMqQueueClient(IRabbitMqConnection connection) {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            var queue = _connection.GetChannel(target);
            var header = queue.CreateBasicProperties();
            if (properties != null) {
                foreach (var prop in properties) {
                    header.Headers.Add(prop.Key, prop.Value);
                }
                if (properties.TryGetValue(EventProperties.ContentType, out var contentType)) {
                    header.ContentType = contentType;
                }
            }
            queue.BasicPublish("", target, header, payload.AsMemory());
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Send<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            _ = SendAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var queue = _connection.GetChannel(target);
            queue.BasicPublish("", target,
                Set(queue.CreateBasicProperties(), contentType, eventSchema, contentEncoding),
                data.AsMemory());
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            var queue = _connection.GetChannel(target);
            var publish = queue.CreateBasicPublishBatch();
            foreach (var data in batch) {
                publish.Add("", target, false,
                    Set(queue.CreateBasicProperties(), contentType, eventSchema, contentEncoding),
                    data.AsMemory());
            }
            publish.Publish();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            _ = SendEventAsync(target, data, contentType, eventSchema, contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <summary>
        /// Helper to create header
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static IBasicProperties Set(IBasicProperties properties, string contentType,
            string eventSchema, string contentEncoding) {
            properties.ContentType = contentType;
            properties.Headers.Add(EventProperties.ContentType, contentType);
            properties.Headers.Add(EventProperties.ContentEncoding, contentEncoding);
            properties.Headers.Add(EventProperties.EventSchema, eventSchema);
            return properties;
        }

        private readonly IRabbitMqConnection _connection;
    }
}
