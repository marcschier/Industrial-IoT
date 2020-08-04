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
        public Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            using (var ev = new EventData(payload)) {
                if (properties != null) {
                    foreach (var prop in properties) {
                        ev.Properties.Add(prop.Key, prop.Value);
                    }
                }
                var client = GetClient(target);
                if (partitionKey != null) {
                    return client.SendAsync(ev, partitionKey);
                }
                return client.SendAsync(ev);
            }
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            using (var ev = CreateEvent(data, contentType, eventSchema, contentEncoding)) {
                var client = GetClient(target);
                return client.SendAsync(ev);
            }
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, IEnumerable<byte[]> batch, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var events = batch
                .Select(b => CreateEvent(b, contentType, eventSchema, contentEncoding))
                .ToList();
            try {
                var client = GetClient(target);
                return client.SendAsync(events);
            }
            finally {
                events.ForEach(e => e?.Dispose());
            }
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
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static EventData CreateEvent(byte[] data, string contentType,
            string eventSchema, string contentEncoding) {
            var ev = new EventData(data);
            ev.Properties.Add(EventProperties.ContentEncoding, contentEncoding);
            ev.Properties.Add(EventProperties.ContentType, contentType);
            ev.Properties.Add(EventProperties.EventSchema, eventSchema);
            return ev;
        }

        /// <summary>
        /// Get client from cache
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private EventHubClient GetClient(string name) {
            var key = name ?? _config.EventHubPath;
            return _cache.GetOrAdd(key, entityPath => {
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
