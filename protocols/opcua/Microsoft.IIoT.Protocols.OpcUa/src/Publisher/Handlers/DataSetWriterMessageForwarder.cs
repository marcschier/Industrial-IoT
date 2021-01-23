// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Processors {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Forwards samples to another event hub
    /// </summary>
    public sealed class DataSetWriterMessageForwarder : IDataSetWriterMessageProcessor {

        /// <summary>
        /// Create forwarder
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        public DataSetWriterMessageForwarder(IEventPublisherClient client,
            IJsonSerializer serializer) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task HandleMessageAsync(PublishedDataSetItemMessageModel sample) {
            // Set timestamp as source timestamp
            var properties = new Dictionary<string, string>() {
                [EventProperties.EventSchema] =
                    MessageSchemaTypes.DataSetWriterMessage
            };
            return _client.PublishAsync(null, _serializer.SerializeToBytes(sample).ToArray(),
                properties.ToEventProperties(), sample.DataSetWriterId);
        }

        private readonly IEventPublisherClient _client;
        private readonly IJsonSerializer _serializer;
    }
}
