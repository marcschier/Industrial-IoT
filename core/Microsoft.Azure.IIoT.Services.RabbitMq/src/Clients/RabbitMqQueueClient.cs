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
    using System.Collections.Concurrent;
    using System.Linq;
    using RabbitMQ.Client;

    /// <summary>
    /// RabbitMq queue client
    /// </summary>
    public sealed class RabbitMqQueueClient : IEventQueueClient, IEventClient {

        /// <summary>
        /// Create queue client
        /// </summary>
        /// <param name="connection"></param>
        public RabbitMqQueueClient(IRabbitMqConnection connection) {
            _connection = connection ??
                throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());
            Send(target, payload, tcs, (t, ex) => {
                if (ex == null) {
                    t.TrySetResult(true);
                }
                else {
                    t.TrySetException(ex);
                }
            }, properties, partitionKey);
            return tcs.Task;
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
            var channel = GetChannel(target);
            channel.Publish(payload.AsMemory(), token, complete, header => {
                if (properties != null) {
                    header.Headers = properties
                        .ToDictionary(k => k.Key, v => (object)v.Value);
                    if (properties.TryGetValue(EventProperties.ContentType,
                        out var contentType)) {
                        header.ContentType = contentType;
                    }
                    if (properties.TryGetValue(EventProperties.ContentEncoding,
                        out var contentEncoding)) {
                        header.ContentEncoding = contentEncoding;
                    }
                }
            });
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());
            SendEvent(target, payload, contentType, eventSchema, contentEncoding,
                tcs, (t, ex) => {
                    if (ex == null) {
                        t.TrySetResult(true);
                    }
                    else {
                        t.TrySetException(ex);
                    }
                });
            return tcs.Task;
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
            var channel = await Task.Run(() => GetChannel(target)).ConfigureAwait(false);
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());
            channel.Publish(batch.Select(b => (ReadOnlyMemory<byte>)b.AsMemory()),
                tcs, (t, ex) => {
                    if (ex == null) {
                        t.TrySetResult(true);
                    }
                    else {
                        t.TrySetException(ex);
                    }
                },
                header => Set(header, target, contentType, eventSchema, contentEncoding));
            await tcs.Task;
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
            var channel = GetChannel(target);
            channel.Publish(payload.AsMemory(), token, complete, header =>
                Set(header, target, contentType, eventSchema, contentEncoding));
        }

        /// <summary>
        /// Helper to create header
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="target"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static void Set(IBasicProperties properties, string target,
            string contentType, string eventSchema, string contentEncoding) {
            properties.ContentType = contentType;
            properties.ContentEncoding = contentEncoding;
            // properties.Type = eventSchema;
            properties.Headers ??= new Dictionary<string, object>();
            properties.Headers.Add(EventProperties.Target, target);
            if (!string.IsNullOrEmpty(contentType)) {
                properties.Headers.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                properties.Headers.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                properties.Headers.Add(EventProperties.EventSchema, eventSchema);
            }
        }

        /// <summary>
        /// Get producer channel
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private IRabbitMqChannel GetChannel(string target) {
            // Get channel from channel cache
            return _producers.GetOrAdd(target, key => {
                var channel = key.Split('/')[0];
                return _connection.GetChannel(channel);
            });
        }

        private readonly IRabbitMqConnection _connection;
        private readonly ConcurrentDictionary<string, IRabbitMqChannel> _producers =
          new ConcurrentDictionary<string, IRabbitMqChannel>();
    }
}
