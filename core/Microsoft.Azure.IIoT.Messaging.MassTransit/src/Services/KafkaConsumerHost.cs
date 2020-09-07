// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using Confluent.Kafka;
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Implementation of event processor host interface to host event
    /// processors.
    /// </summary>
    public sealed class KafkaConsumerHost : IDisposable, IEventProcessingHost, IHostProcess {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="identity"></param>
        public KafkaConsumerHost(IEventProcessingHandler consumer,
            IKafkaConsumerConfig config, ILogger logger, IProcessIdentity identity) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _consumerId = identity.Id ?? Guid.NewGuid().ToString();
            _interval = (long?)config.CheckpointInterval?.TotalMilliseconds
               ?? long.MaxValue;
            _lock = new SemaphoreSlim(1, 1);
            _sw = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_host != null) {
                    _logger.Debug("Consumer host already running.");
                    return;
                }
                _cts = new CancellationTokenSource();
                try {
                    _logger.Debug("Starting consumer host...");
                    _host = Task.Run(() => ConsumeAsync(_cts.Token));
                    _logger.Information("Consumer host started.");
                }
                finally {
                    _cts.Dispose();
                    _host = null;
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, "Error starting event processor host.");
                _host = null;
                throw ex;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync();
            try {
                if (_host == null) {
                    return;
                }
                try {
                    _logger.Debug("Stopping consumer host...");
                    _cts.Cancel();
                    await _host;
                    _logger.Information("Consumer host stopped.");
                }
                finally {
                    _cts.Dispose();
                    _host = null;
                }
            }
            catch (Exception ex) {
                _logger.Warning(ex, "Error stopping consumer host");
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Start() {
            StartAsync().Wait();
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
            _lock.Dispose();
        }

        /// <summary>
        /// Consumer loop
        /// </summary>
        /// <param name="ct"></param>
        private async Task ConsumeAsync(CancellationToken ct) {
            _sw.Restart();
            var messagesCount = 0;
            var config = new ConsumerConfig {
                BootstrapServers = _config.BootstrapServers,
                GroupId = _config.ConsumerGroup,
                AutoOffsetReset = _config.InitialReadFromEnd ?
                    AutoOffsetReset.Latest : AutoOffsetReset.Error,
                EnableAutoCommit = false,
                // ...
            };
            var topic = _config.Topic ?? ".*";
            while (!ct.IsCancellationRequested) {
                try {
                    using (var consumer = new ConsumerBuilder<string, byte[]>(config).Build()) {
                        _logger.Information("Starting consumer {consumerId} on {topic}...",
                            _consumerId, topic);
                        consumer.Subscribe(topic);
                        while (!ct.IsCancellationRequested) {
                            var result = consumer.Consume(ct);
                            var ev = result.Message;
                            messagesCount++;

                            if (_config.SkipEventsOlderThan != null &&
                                ev.Timestamp.UtcDateTime +
                                    _config.SkipEventsOlderThan < DateTime.UtcNow) {
                                // Skip this one and catch up
                                continue;
                            }

                            await _consumer.HandleAsync(ev.Value, new EventHeader(ev.Headers),
                                () => CommitAsync(consumer, result));

                            // Commit if needed
                            if (_sw.ElapsedMilliseconds >= _interval) {
                                try {
                                    _logger.Debug("Checkpointing consumer {consumerId}...",
                                        _consumerId);
                                    await CommitAsync(consumer, result);
                                    _sw.Restart();
                                }
                                catch (Exception ex) {
                                    _logger.Warning(ex, "Failed checkpointing {consumerId}...",
                                        _consumerId);
                                    if (_sw.ElapsedMilliseconds >= 2 * _interval) {
                                        // Give up checkpointing after trying a couple more times
                                        _sw.Restart();
                                    }
                                }
                            }
                            await Try.Async(_consumer.OnBatchCompleteAsync);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception error) {
                    // Exception - report and continue
                    _logger.Warning(error, "Consumer {consumerId} encountered error...",
                        _consumerId);
                    continue;
                }
            }
            _logger.Information("Exiting consumer {consumerId} on {topic}...",
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
                _logger.Debug("Commit consumer {id} {memberId}...", _consumerId, consumer.MemberId);
                consumer.Commit(result);
            }
            catch (Exception ex) {
                _logger.Warning(ex, "Failed to commit consumer {id} {memberId}...", _consumerId,
                    consumer.MemberId);
            }
            finally {
                _sw.Restart();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Wraps the properties into a string dictionary
        /// </summary>
        private class EventHeader : IDictionary<string, string> {

            /// <summary>
            /// Create properties wrapper
            /// </summary>
            /// <param name="headers"></param>
            internal EventHeader(Headers headers) {
                _headers = headers ?? new Headers();
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
                set => Add(key, value);
            }

            /// <inheritdoc/>
            public void Add(string key, string value) {
                _headers.Add(key, Encoding.UTF8.GetBytes(value));
            }

            /// <inheritdoc/>
            public bool ContainsKey(string key) {
                return _headers.TryGetLastBytes(key, out _);
            }

            /// <inheritdoc/>
            public bool Remove(string key) {
                throw new NotSupportedException("Cannot remove items");
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
                _headers.Add(item.Key, Encoding.UTF8.GetBytes(item.Value));
            }

            /// <inheritdoc/>
            public void Clear() {
                Keys.ToList().ForEach(x => _headers.Remove(x));
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
                if (Contains(item)) {
                    return Remove(item.Key);
                }
                return false;
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

        private CancellationTokenSource _cts;
        private Task _host;
        private readonly SemaphoreSlim _lock;
        private readonly ILogger _logger;
        private readonly IEventProcessingHandler _consumer;
        private readonly IKafkaConsumerConfig _config;
        private readonly string _consumerId;
        private readonly long? _interval;
        private readonly Stopwatch _sw;
    }
}
