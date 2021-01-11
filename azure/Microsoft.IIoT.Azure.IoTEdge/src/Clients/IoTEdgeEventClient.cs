// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.IIoT.Azure.IoTEdge;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event client implementation
    /// </summary>
    public sealed class IoTEdgeEventClient : IEventClient {

        /// <summary>
        /// Create Event client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="identity"></param>
        public IoTEdgeEventClient(IIoTEdgeDeviceClient client, IIdentity identity = null) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _identity = identity;
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }

            // TODO: Must batch based on sizes since a batch must be less than max

            var messages = batch
                .Select(ev =>
                     CreateMessage(ev, contentEncoding, contentType, eventSchema))
                .ToList();
            try {
                await _client.SendEventBatchAsync(target, messages, ct).ConfigureAwait(false);
            }
            finally {
                messages.ForEach(m => m?.Dispose());
            }
        }

        /// <inheritdoc/>
        public async Task SendEventAsync(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentNullException(nameof(target));
            }
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            using (var msg = CreateMessage(data, contentEncoding, contentType, eventSchema)) {
                await _client.SendEventAsync(target, msg, ct).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] data, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentNullException(nameof(target));
            }
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            var t = SendEventAsync(target, data, contentType, eventSchema, contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait();
        }

        /// <summary>
        /// Create message
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <returns></returns>
        private Message CreateMessage(byte[] data, string contentEncoding,
            string contentType, string eventSchema) {
            var msg = new Message(data) {

                ContentType = contentType,
                ContentEncoding = contentEncoding,
                // TODO - setting CreationTime causes issues in the Azure IoT java SDK
                // revert the comment when the issue is fixed
                //  CreationTimeUtc = DateTime.UtcNow
            };
            if (!string.IsNullOrEmpty(contentEncoding)) {
                msg.Properties.Add(EventProperties.ContentEncoding, contentEncoding);
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                msg.Properties.Add(EventProperties.EventSchema, eventSchema);
            }
            return msg;
        }

        private readonly IIoTEdgeDeviceClient _client;
        private readonly IIdentity _identity;
    }
}
