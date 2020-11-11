// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Kafka.Services {
    using Microsoft.Azure.IIoT.Extensions.Kafka.Clients;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Text;
    using Confluent.Kafka;

    /// <summary>
    /// Implementation of event processor host interface to host event
    /// processors.
    /// </summary>
    public sealed class KafkaConsumerHost : HostProcess, IEventProcessingHost {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="consumer"></param>
        /// <param name="server"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="identity"></param>
        public KafkaConsumerHost(IEventConsumer consumer, IKafkaAdminClient admin,
            IOptions<KafkaServerOptions> server, IOptions<KafkaConsumerOptions> config,
            ILogger logger, IProcessIdentity identity) : base(logger, "Kafka") {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _admin = admin ?? throw new ArgumentNullException(nameof(admin));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _consumerId = identity.Id ?? Guid.NewGuid().ToString();
            _interval = (int?)config.Value.CheckpointInterval?.TotalMilliseconds;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken ct) {

            var config = new ConsumerConfig {
                BootstrapServers = _server.Value.BootstrapServers,
                GroupId = _config.Value.ConsumerGroup,
                AutoOffsetReset = _config.Value.InitialReadFromEnd ?
                    AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = true,
                AutoCommitIntervalMs = _interval,
                // ...
            };
            var topic = _config.Value.ConsumerTopic ?? "^.*";
            if (!topic.StartsWith("^")) {
                await _admin.EnsureTopicExistsAsync(topic).ConfigureAwait(false);
            }
            while (!ct.IsCancellationRequested) {
                try {
                    using (var consumer = new ConsumerBuilder<string, byte[]>(config)
                        .SetErrorHandler(OnError)
                        .SetStatisticsHandler(OnMetrics)
                        .SetLogHandler((_, m) => _logger.Log(m))
                        .Build()) {
                        _logger.LogInformation("Starting consumer {consumerId} on {topic}...",
                            _consumerId, topic);
                        consumer.Subscribe(topic);
                        while (!ct.IsCancellationRequested) {
                            var result = consumer.Consume(ct);
                            var ev = result.Message;
                            if (result.Topic == "__consumer_offsets") {
                                continue;
                            }
                            if (_config.Value.SkipEventsOlderThan != null &&
                                ev.Timestamp.UtcDateTime +
                                    _config.Value.SkipEventsOlderThan < DateTime.UtcNow) {
                                // Skip this one and catch up
                                continue;
                            }
                            await _consumer.HandleAsync(ev.Value, new EventHeader(
                                ev.Headers, result.Topic),
                                    () => CommitAsync(consumer, result)).ConfigureAwait(false);
                            await Try.Async(_consumer.OnBatchCompleteAsync).ConfigureAwait(false);
                        }
                        consumer.Close();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception error) {
                    // Exception - report and continue
                    _logger.LogWarning(error, "Consumer {consumerId} encountered error...",
                        _consumerId);
                    continue;
                }
            }
            _logger.LogInformation("Exiting consumer {consumerId} on {topic}...",
                _consumerId, topic);
        }

        /// <summary>
        /// Perform commmit
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Task CommitAsync(IConsumer<string, byte[]> consumer,
            ConsumeResult<string, byte[]> result) {
            try {
                _logger.LogDebug("Commit consumer {id} {memberId}...", _consumerId,
                    consumer.MemberId);
                consumer.Commit(result);
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to commit consumer {id} {memberId}...",
                    _consumerId, consumer.MemberId);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle error
        /// </summary>
        /// <param name="client"></param>
        /// <param name="error"></param>
        private void OnError(IConsumer<string, byte[]> client, Error error) {
            // Todo
        }

        /// <summary>
        /// Handle metrics
        /// </summary>
        /// <param name="client"></param>
        /// <param name="json"></param>
        private void OnMetrics(IConsumer<string, byte[]> client, string json) {
        }

        /// <summary>
        /// Wraps the properties into a string dictionary
        /// </summary>
        private class EventHeader : IDictionary<string, string> {

            /// <summary>
            /// Create properties wrapper
            /// </summary>
            /// <param name="headers"></param>
            /// <param name="topic"></param>
            internal EventHeader(Headers headers, string topic) {
                _headers = headers ?? new Headers();
                if (topic != null) {
                    _headers.Add("x-topic", Encoding.UTF8.GetBytes(topic));
                }
            }

            /// <inheritdoc/>
            public ICollection<string> Keys => _headers.Select(x => x.Key).ToList();

            /// <inheritdoc/>
            public ICollection<string> Values => Keys.Select(x => this[x]).ToList();

            /// <inheritdoc/>
            public int Count => _headers.Count;

            /// <inheritdoc/>
            public bool IsReadOnly => true;

            /// <inheritdoc/>
            public string this[string key] {
                get {
                    if (TryGetValue(key, out var result)) {
                        return result;
                    }
                    return null;
                }
                set => throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public void Add(string key, string value) {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public bool ContainsKey(string key) {
                return _headers.TryGetLastBytes(key, out _);
            }

            /// <inheritdoc/>
            public bool Remove(string key) {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public bool TryGetValue(string key, out string value) {
                if (_headers.TryGetLastBytes(key, out var result)) {
                    value = Encoding.UTF8.GetString(result);
                    return true;
                }
                value = null;
                return false;
            }

            /// <inheritdoc/>
            public void Add(KeyValuePair<string, string> item) {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public void Clear() {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public bool Contains(KeyValuePair<string, string> item) {
                if (TryGetValue(item.Key, out var value)) {
                    return value == item.Value.ToString();
                }
                return false;
            }

            /// <inheritdoc/>
            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
                var index = arrayIndex;
                foreach (var item in this) {
                    if (index >= array.Length) {
                        return;
                    }
                    array[index++] = item;
                }
            }

            /// <inheritdoc/>
            public bool Remove(KeyValuePair<string, string> item) {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
                return _headers
                    .Select(x => new KeyValuePair<string, string>(
                        x.Key, Encoding.UTF8.GetString(x.GetValueBytes())))
                    .GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() {
                return _headers
                    .Select(x => new KeyValuePair<string, string>(
                        x.Key, Encoding.UTF8.GetString(x.GetValueBytes())))
                    .GetEnumerator();
            }

            private readonly Headers _headers;
        }

        private readonly ILogger _logger;
        private readonly IKafkaAdminClient _admin;
        private readonly IEventConsumer _consumer;
        private readonly IOptions<KafkaConsumerOptions> _config;
        private readonly IOptions<KafkaServerOptions> _server;
        private readonly string _consumerId;
        private readonly int? _interval;
    }
}
