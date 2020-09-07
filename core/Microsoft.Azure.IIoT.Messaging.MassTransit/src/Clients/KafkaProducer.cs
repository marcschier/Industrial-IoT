// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka.Clients {
    using Microsoft.Azure.IIoT.Messaging.Kafka;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Text;
    using Confluent.Kafka;

    /// <summary>
    /// Event hub namespace client
    /// </summary>
    public sealed class KafkaProducer : IEventQueueClient, IEventClient, IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="identity"></param>
        public KafkaProducer(IKafkaProducerConfig config, IProcessIdentity identity) {
            if (string.IsNullOrEmpty(config.BootstrapServers)) {
                throw new ArgumentException(nameof(config));
            }
            _producer = new ProducerBuilder<string, byte[]>(new ProducerConfig {
                BootstrapServers = config.BootstrapServers,
                ClientId = identity.Id ?? Dns.GetHostName(),
                // ...
            }).Build();
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            var ev = new Message<string, byte[]> {
                Key = partitionKey,
                Value = payload,
                Headers = CreateHeader(properties)
            };
            await _producer.ProduceAsync(target, ev, ct);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var ev = new Message<string, byte[]> {
                Key = eventSchema,
                Value = data,
                Headers = CreateHeader(contentType, eventSchema, contentEncoding)
            };
            await _producer.ProduceAsync(target, ev, ct);
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var header = CreateHeader(contentType, eventSchema, contentEncoding);
            foreach (var data in batch) {
                var ev = new Message<string, byte[]> {
                    Key = eventSchema,
                    Value = data,
                    Headers = header
                };
                _producer.Produce(target, ev);
            }
            return Task.Run(() => _producer.Flush(ct));
        }

        /// <inheritdoc/>
        public void Dispose() {
            _producer.Dispose();
        }

        /// <summary>
        /// Create hader
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static Headers CreateHeader(IDictionary<string, string> properties) {
            var header = new Headers();
            if (properties != null) {
                foreach (var prop in properties) {
                    header.Add(prop.Key, Encoding.UTF8.GetBytes(prop.Value));
                }
            }
            return header;
        }

        /// <summary>
        /// Helper to create header
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static Headers CreateHeader(string contentType, string eventSchema,
            string contentEncoding) {
            var header = new Headers();
            if (!string.IsNullOrEmpty(contentType)) {
                header.Add(EventProperties.ContentType, Encoding.UTF8.GetBytes(contentType));
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                header.Add(EventProperties.ContentEncoding, Encoding.UTF8.GetBytes(contentEncoding));
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                header.Add(EventProperties.EventSchema, Encoding.UTF8.GetBytes(eventSchema));
            }
            return header;
        }

        private readonly IProducer<string, byte[]> _producer;
    }
}
