// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Subscriber.Processors {
    using Microsoft.Azure.IIoT.Platform.Subscriber.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Forwards samples to another event hub
    /// </summary>
    public sealed class MonitoredItemSampleForwarder : ISubscriberMessageProcessor {

        /// <summary>
        /// Create forwarder
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        public MonitoredItemSampleForwarder(IEventQueueClient client,
            IJsonSerializer serializer) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task HandleSampleAsync(MonitoredItemMessageModel sample) {
            // Set timestamp as source timestamp
            var properties = new Dictionary<string, string>() {
                [CommonProperties.EventSchemaType] =
                    Core.MessageSchemaTypes.MonitoredItemMessageModelJson
            };
            return _client.SendAsync(null, _serializer.SerializeToBytes(sample).ToArray(),
                properties, sample.DataSetWriterId);
        }

        /// <inheritdoc/>
        public Task HandleMessageAsync(DataSetMessageModel message) {
            var properties = new Dictionary<string, string>() {
                [CommonProperties.EventSchemaType] =
                    Core.MessageSchemaTypes.NetworkMessageModelJson
            };
            return _client.SendAsync(null, _serializer.SerializeToBytes(message).ToArray(),
                properties, message.DataSetWriterId);
        }

        private readonly IEventQueueClient _client;
        private readonly IJsonSerializer _serializer;
    }
}
