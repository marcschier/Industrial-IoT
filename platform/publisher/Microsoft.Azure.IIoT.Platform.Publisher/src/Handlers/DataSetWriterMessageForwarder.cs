// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Processors {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
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
        public DataSetWriterMessageForwarder(IEventQueueClient client,
            IJsonSerializer serializer) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task HandleMessageAsync(PublishedDataSetItemMessageModel sample) {
            // Set timestamp as source timestamp
            var properties = new Dictionary<string, string>() {
                [CommonProperties.EventSchemaType] =
                    MessageSchemaTypes.DataSetWriterMessage
            };
            return _client.SendAsync(null, _serializer.SerializeToBytes(sample).ToArray(),
                properties, sample.DataSetWriterId);
        }

        private readonly IEventQueueClient _client;
        private readonly IJsonSerializer _serializer;
    }
}
