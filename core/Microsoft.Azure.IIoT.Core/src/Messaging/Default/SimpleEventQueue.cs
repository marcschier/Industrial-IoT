// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Default {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Exceptions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Linq;
    using System;

    /// <summary>
    /// Event hub namespace client
    /// </summary>
    public sealed class SimpleEventQueue : IEventQueueClient, IEventClient, IEventConsumer {

        /// <summary>
        /// Create queue
        /// </summary>
        public SimpleEventQueue() {
            _control = new BlockingCollection<Message>();
            _targets = new ConcurrentDictionary<int, BlockingCollection<Message>> {
                [ 0 ] = _control
            };
        }

        /// <inheritdoc/>
        public Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var partition = GetOrAddPartition(partitionKey ?? target);
            if (!partition.TryAdd(new Message {
                Value = payload,
                Target = target,
                Properties = properties ?? new Dictionary<string, string>()
            })) {
                throw new ExternalDependencyException("Failed to queue event");
            }
            return Task.CompletedTask;
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
            _ = SendAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            return SendAsync(target, payload,
                CreateProperties(contentType, eventSchema, contentEncoding), eventSchema, ct);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding,
            CancellationToken ct) {
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var properties = CreateProperties(contentType, eventSchema, contentEncoding);
            foreach (var payload in batch) {
                await SendAsync(target, payload, properties, eventSchema, ct);
            }
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
            _ = SendEventAsync(target, payload, contentType, eventSchema, contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<(string, byte[], IDictionary<string, string>)>> ConsumeAsync(
            CancellationToken ct) {
            return Task.Run(() => {
                var results = new List<(string, byte[], IDictionary<string, string>)>();
                while (true) {
                    var queues = _targets.Values.ToArray();
                    if (-1 != BlockingCollection<Message>.TryTakeFromAny(queues,
                        out var message, Timeout.Infinite, ct)) {
                        if (message.Reset) {
                            continue; // Received a control message - re-acquire all queues
                        }

                        results.Add((message.Target, message.Value, message.Properties));
                        while (-1 != BlockingCollection<Message>.TryTakeFromAny(queues,
                            out message, 0, ct)) {
                            if (message.Reset) {
                                break; // Control message - stop and return what we have
                            }
                            results.Add((message.Target, message.Value, message.Properties));
                            if (results.Count > 20) { // max batch size
                                break;
                            }
                        }
                    }
                    break;
                }
                return (IEnumerable<(string, byte[], IDictionary<string, string>)>)results;
            }, ct);
        }

        /// <summary>
        /// Gets or adds new queue
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
        private BlockingCollection<Message> GetOrAddPartition(string partition) {
            var created = false;
            var partitionIndex = 1;
            if (!string.IsNullOrEmpty(partition)) {
                partitionIndex = (int)((uint)partition.ToLowerInvariant().GetHashCode() % 63) + 1;
            }
            var queue = _targets.GetOrAdd(partitionIndex, index => {
                created = true;
                return new BlockingCollection<Message>();
            });
            if (created) { // New queue created - reset
                _control.TryAdd(new Message { Reset = true });
            }
            return queue;
        }

        /// <summary>
        /// Create properties
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private IDictionary<string, string> CreateProperties(string contentType,
            string eventSchema, string contentEncoding) {
            var header = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(contentType)) {
                header.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                header.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                header.Add(EventProperties.EventSchema, eventSchema);
            }
            return header;
        }

        /// <summary>
        /// Message object
        /// </summary>
        internal sealed class Message {

            /// <summary>
            /// Control message
            /// </summary>
            public bool Reset { get; internal set; }

            /// <summary>
            /// Target resource
            /// </summary>
            public string Target { get; internal set; }

            /// <summary>
            /// Value
            /// </summary>
            public byte[] Value { get; internal set; }

            /// <summary>
            /// Properties
            /// </summary>
            public IDictionary<string, string> Properties { get; internal set; }
        }

        private readonly BlockingCollection<Message> _control;
        private readonly ConcurrentDictionary<int, BlockingCollection<Message>> _targets;
    }
}
