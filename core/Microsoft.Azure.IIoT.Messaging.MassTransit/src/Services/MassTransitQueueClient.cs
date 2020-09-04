// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MassTransit;

    /// <summary>
    /// Service bus queue client
    /// </summary>
    public sealed class MassTransitQueueClient : IEventQueueClient, IEventClient {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="sender"></param>
        public MassTransitQueueClient(ISendEndpointProvider sender) {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /// <inheritdoc/>
        public async Task SendAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey, CancellationToken ct) {
            var client = await _sender.GetSendEndpoint(new Uri(target));
            await client.Send(payload, context => Add(context.Headers, properties,
                null, null, null));
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            var client = await _sender.GetSendEndpoint(new Uri(target));
            await client.Send(data, context => Add(context.Headers, null, contentType,
                eventSchema, contentEncoding));
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            var client = await _sender.GetSendEndpoint(new Uri(target));
            foreach (var data in batch) {
                await client.Send(data, context => Add(context.Headers, null, contentType,
                    eventSchema, contentEncoding));
            }
        }

        /// <summary>
        /// Helper to add properties to message
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="properties"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        ///
        private void Add(SendHeaders headers, IDictionary<string, string> properties,
            string contentType, string eventSchema, string contentEncoding) {
            if (properties != null) {
                foreach (var prop in properties) {
                    headers.Set(prop.Key, prop.Value);
                }
            }
            if (contentType != null) {
                headers.Set(EventProperties.ContentType, contentType);
            }
            if (contentEncoding != null) {
                headers.Set(EventProperties.ContentEncoding, contentEncoding);
            }
            if (eventSchema != null) {
                headers.Set(EventProperties.EventSchema, eventSchema);
            }
        }

        private readonly ISendEndpointProvider _sender;
    }
}
