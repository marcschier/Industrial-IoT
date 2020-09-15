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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Text;
    using System.Collections.Concurrent;
    using System.Linq;
    using Confluent.Kafka;
    using Confluent.Kafka.Admin;

    /// <summary>
    /// Kafka producer
    /// </summary>
    public sealed class KafkaProducer : IEventQueueClient, IEventClient, IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="identity"></param>
        public KafkaProducer(IKafkaServerConfig config, IProcessIdentity identity = null) {
            if (string.IsNullOrEmpty(config?.BootstrapServers)) {
                throw new ArgumentException(nameof(config));
            }
            _config = config;
            _clientId = identity?.Id ?? Dns.GetHostName();
            _producer = new ProducerBuilder<string, byte[]>(ClientConfig<ProducerConfig>())
                .Build();
            _admin = new AdminClientBuilder(ClientConfig<AdminClientConfig>())
                .Build();
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var ev = new Message<string, byte[]> {
                Key = partitionKey ?? target,
                Value = payload,
                Headers = CreateHeader(properties)
            };
            var topic = await EnsureTopicAsync(target);
            await _producer.ProduceAsync(topic, ev, ct);
        }

        /// <inheritdoc/>
        public void Send<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
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
                Key = partitionKey ?? target,
                Value = payload,
                Headers = CreateHeader(properties)
            };
            var topic = EnsureTopicAsync(target).Result;
            _producer.Produce(topic, ev,
                report => complete(token, report.Error == null ?
                    null : new ExternalDependencyException(report.Error.Reason)));
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (target == null) {
                target = "default";
            }
            var ev = new Message<string, byte[]> {
                Key = eventSchema ?? target,
                Value = payload,
                Headers = CreateHeader(contentType, eventSchema, contentEncoding)
            };
            var topic = await EnsureTopicAsync(target);
            await _producer.ProduceAsync(topic, ev, ct);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var header = CreateHeader(contentType, eventSchema, contentEncoding);
            var topic = await EnsureTopicAsync(target);
            foreach (var payload in batch) {
                var ev = new Message<string, byte[]> {
                    Key = eventSchema ?? target,
                    Value = payload,
                    Headers = header
                };
                _producer.Produce(topic, ev);
            }
            await Task.Run(() => _producer.Flush(ct));
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
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
                Key = eventSchema ?? target,
                Value = payload,
                Headers = CreateHeader(contentType, eventSchema, contentEncoding)
            };
            var topic = EnsureTopicAsync(target).Result;
            _producer.Produce(topic, ev,
                report => complete(token, report.Error == null ?
                    null : new ExternalDependencyException(report.Error.Reason)));
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

        /// <summary>
        /// Ensure topic
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Task<string> EnsureTopicAsync(string target) {
            return _topics.GetOrAdd(target, async k => {
                var topic = target.Replace('/', '.');
                await _admin.CreateTopicsAsync(
                    new TopicSpecification {
                        Name = topic,
                        NumPartitions = _config.Partitions,
                        ReplicationFactor = (short)_config.ReplicaFactor,
                    }.YieldReturn(),
                    new CreateTopicsOptions {
                        OperationTimeout = TimeSpan.FromSeconds(30),
                        RequestTimeout = TimeSpan.FromSeconds(30)
                    });
                return topic;
            });
        }

        /// <summary>
        /// Create configuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T ClientConfig<T>()
            where T : ClientConfig, new() {
            return new T {
                BootstrapServers = _config.BootstrapServers,
                ClientId = _clientId,
                // ...
            };
        }

        private readonly ConcurrentDictionary<string, Task<string>> _topics =
            new ConcurrentDictionary<string, Task<string>>();
        private readonly IProducer<string, byte[]> _producer;
        private readonly IAdminClient _admin;
        private readonly IKafkaServerConfig _config;
        private readonly string _clientId;
    }
}
