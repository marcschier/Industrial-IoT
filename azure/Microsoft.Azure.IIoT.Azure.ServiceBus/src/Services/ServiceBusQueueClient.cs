// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.ServiceBus;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service bus queue client
    /// </summary>
    public sealed class ServiceBusQueueClient : IEventQueueClient, IEventClient {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="factory"></param>
        public ServiceBusQueueClient(IServiceBusClientFactory factory) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            var msg = new Message(payload);
            if (properties != null) {
                foreach (var prop in properties) {
                    msg.UserProperties.Add(prop.Key, prop.Value);
                }
                if (properties.TryGetValue(EventProperties.ContentType,
                    out var contentType)) {
                    msg.ContentType = contentType;
                }
            }
            var client = await _factory.CreateOrGetGetQueueClientAsync(target);
            await client.SendAsync(msg);
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var client = await _factory.CreateOrGetGetQueueClientAsync(target);
            await client.SendAsync(CreateMessage(data, contentType, eventSchema, contentEncoding));
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            var client = await _factory.CreateOrGetGetQueueClientAsync(target);
            await client.SendAsync(batch
                .Select(b => CreateMessage(b, contentType, eventSchema, contentEncoding))
                .ToList());
        }

        /// <summary>
        /// Helper to create event from buffer and content type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static Message CreateMessage(byte[] data, string contentType,
            string eventSchema, string contentEncoding) {
            var ev = new Message(data) {
                ContentType = contentType
            };
            ev.UserProperties.Add(EventProperties.ContentType, contentType);
            ev.UserProperties.Add(EventProperties.ContentEncoding, contentEncoding);
            ev.UserProperties.Add(EventProperties.EventSchema, eventSchema);
            return ev;
        }

        private readonly IServiceBusClientFactory _factory;
    }
}
