// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging.Clients {
    using Microsoft.IIoT.Extensions.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event client to publisher client adapter
    /// </summary>
    public sealed class EventClientAdapter : IEventClient {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="client"></param>
        public EventClientAdapter(IEventPublisherClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            return _client.PublishAsync(target, payload,
                CreateProperties(contentType, eventSchema, contentEncoding), eventSchema, ct);
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            return _client.PublishAsync(target, batch,
                CreateProperties(contentType, eventSchema, contentEncoding), eventSchema, ct);
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
            _client.Publish(target, payload, token, complete,
                CreateProperties(contentType, eventSchema, contentEncoding), eventSchema);
        }

        /// <summary>
        /// Create properties
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static IEventProperties CreateProperties(
            string contentType, string eventSchema, string contentEncoding) {
            var properties = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(contentType)) {
                properties.Add(EventProperties.ContentType, contentType);
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                properties.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                properties.Add(EventProperties.EventSchema, eventSchema);
            }
            return properties.ToEventProperties();
        }

        private readonly IEventPublisherClient _client;
    }
}