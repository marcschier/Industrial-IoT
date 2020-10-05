// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Clients {
    using Microsoft.Azure.IIoT.Services.Kafka;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Text;
    using Confluent.Kafka;

    /// <summary>
    /// Kafka producer
    /// </summary>
    public sealed class KafkaProducerClient : IEventQueueClient, IEventClient,
        IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="identity"></param>
        /// <param name="admin"></param>
        public KafkaProducerClient(IKafkaServerConfig config, IKafkaAdminClient admin,
            ILogger logger, IProcessIdentity identity = null) {
            _admin = admin ?? throw new ArgumentNullException(nameof(admin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _producer = new ProducerBuilder<string, byte[]>(
                config.ToClientConfig<ProducerConfig>(
                    identity?.Id ?? Dns.GetHostName()))
                .SetErrorHandler(OnError)
                .SetStatisticsHandler(OnMetrics)
                .SetLogHandler((_, m) => _logger.Log(m))
                .Build();
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var ev = new Message<string, byte[]> {
                Key = GetKey(target, null, partitionKey),
                Value = payload,
                Headers = CreateHeader(target, properties)
            };
            var topic = await EnsureTopicAsync(target).ConfigureAwait(false);
            await _producer.ProduceAsync(topic, ev, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Send<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
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
            var ev = new Message<string, byte[]> {
                Key = GetKey(target, null, partitionKey),
                Value = payload,
                Headers = CreateHeader(target, properties)
            };
            var topic = EnsureTopicAsync(target).Result;
            _producer.Produce(topic, ev,
                report => complete(token, report.Error.IsError ?
                    new ExternalDependencyException(report.Error.Reason) : null));
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
            var ev = new Message<string, byte[]> {
                Key = GetKey(target, eventSchema, null),
                Value = payload,
                Headers = CreateHeader(target, contentType, eventSchema, contentEncoding)
            };
            var topic = await EnsureTopicAsync(target).ConfigureAwait(false);
            await _producer.ProduceAsync(topic, ev, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var header = CreateHeader(target, contentType, eventSchema, contentEncoding);
            var topic = await EnsureTopicAsync(target).ConfigureAwait(false);
            foreach (var payload in batch) {
                var ev = new Message<string, byte[]> {
                    Key = GetKey(target, eventSchema, null),
                    Value = payload,
                    Headers = header
                };
                _producer.Produce(topic, ev);
            }
            await Task.Run(() => _producer.Flush(ct), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
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
            var ev = new Message<string, byte[]> {
                Key = GetKey(target, eventSchema, null),
                Value = payload,
                Headers = CreateHeader(target, contentType, eventSchema, contentEncoding)
            };
            var topic = EnsureTopicAsync(target).Result;
            _producer.Produce(topic, ev,
                report => complete(token, report.Error.IsError ?
                    new ExternalDependencyException(report.Error.Reason) : null));
        }

        /// <inheritdoc/>
        public void Dispose() {
            _producer?.Dispose();
            _topics.Clear();
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
            return key.ToLowerInvariant();
        }

        /// <summary>
        /// Create hader
        /// </summary>
        /// <param name="target"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static Headers CreateHeader(string target, IDictionary<string, string> properties) {
            var header = new Headers();
            if (properties != null) {
                foreach (var prop in properties) {
                    header.Add(prop.Key, Encoding.UTF8.GetBytes(prop.Value));
                }
            }
            if (!string.IsNullOrEmpty(target)) {
                header.Add(EventProperties.Target, Encoding.UTF8.GetBytes(target));
            }
            return header;
        }

        /// <summary>
        /// Helper to create header
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static Headers CreateHeader(string target, string contentType, string eventSchema,
            string contentEncoding) {
            var header = new Headers {
                { EventProperties.Target, Encoding.UTF8.GetBytes(target) }
            };
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

        /// <summary>
        /// Ensure topic
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Task<string> EnsureTopicAsync(string target) {
            return _topics.GetOrAdd(target, async k => {
                var topic = target.Split('/')[0];
                await _admin.EnsureTopicExistsAsync(topic).ConfigureAwait(false);
                return topic;
            });
        }

        /// <summary>
        /// Handle error
        /// </summary>
        /// <param name="client"></param>
        /// <param name="error"></param>
        private void OnError(IProducer<string, byte[]> client, Error error) {
            // Todo
        }

        /// <summary>
        /// Handle metrics
        /// </summary>
        /// <param name="client"></param>
        /// <param name="json"></param>
        private void OnMetrics(IProducer<string, byte[]> client, string json) {
        }

        private readonly ConcurrentDictionary<string, Task<string>> _topics =
            new ConcurrentDictionary<string, Task<string>>();
        private readonly IProducer<string, byte[]> _producer;
        private readonly IKafkaAdminClient _admin;
        private readonly ILogger _logger;
    }
}
