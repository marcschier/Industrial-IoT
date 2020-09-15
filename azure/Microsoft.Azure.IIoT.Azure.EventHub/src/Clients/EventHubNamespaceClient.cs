// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Clients {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.EventHubs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Concurrent;

    /// <summary>
    /// Event hub namespace client
    /// </summary>
    public sealed class EventHubNamespaceClient : IEventQueueClient, IEventClient, IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        public EventHubNamespaceClient(IEventHubClientConfig config) {
            if (string.IsNullOrEmpty(config.EventHubConnString)) {
                throw new ArgumentException(nameof(config));
            }
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }


        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            using (var ev = new EventData(payload)) {
                if (properties != null) {
                    foreach (var prop in properties) {
                        ev.Properties.Add(prop.Key, prop.Value);
                    }
                }
                if (!string.IsNullOrEmpty(target)) {
                    ev.Properties.Add(EventProperties.Target, target);
                }
                var client = GetClient(target);
                if (partitionKey != null) {
                    await client.SendAsync(ev, partitionKey);
                }
                else {
                    await client.SendAsync(ev);
                }
            }
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
        public async Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            using (var ev = CreateEvent(data, target, contentType, eventSchema, contentEncoding)) {
                var client = GetClient(target);
                await client.SendAsync(ev);
            }
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            var events = batch
                .Select(b => CreateEvent(b, target, contentType, eventSchema, contentEncoding))
                .ToList();
            try {
                var client = GetClient(target);
                await client.SendAsync(events);
            }
            finally {
                events.ForEach(e => e?.Dispose());
            }
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

        /// <inheritdoc/>
        public void Dispose() {
            foreach (var client in _cache.Values.ToList()) {
                client.Close();
            }
            _cache.Clear();
        }

        /// <summary>
        /// Helper to create event from buffer and content type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static EventData CreateEvent(byte[] data, string target,
            string contentType, string eventSchema, string contentEncoding) {
            var ev = new EventData(data);
            if (!string.IsNullOrEmpty(contentEncoding)) {
                ev.Properties.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(contentType)) {
                ev.Properties.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                ev.Properties.Add(EventProperties.EventSchema, eventSchema);
            }
            if (!string.IsNullOrEmpty(target)) {
                ev.Properties.Add(EventProperties.Target, target);
            }
            return ev;
        }

        /// <summary>
        /// Get client from cache
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private EventHubClient GetClient(string target) {
            return _cache.GetOrAdd(target ?? _config.EventHubPath, entityPath => {
                if (entityPath != _config.EventHubPath) {
                    entityPath = entityPath.Split('/')[0];
                }
                var cs = new EventHubsConnectionStringBuilder(_config.EventHubConnString) {
                    EntityPath = entityPath
                }.ToString();
                return EventHubClient.CreateFromConnectionString(cs);
            });
        }

        private readonly ConcurrentDictionary<string, EventHubClient> _cache =
            new ConcurrentDictionary<string, EventHubClient>();
        private readonly IEventHubClientConfig _config;
    }
}
