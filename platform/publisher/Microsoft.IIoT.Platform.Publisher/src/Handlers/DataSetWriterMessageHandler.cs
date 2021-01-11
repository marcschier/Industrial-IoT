// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Handlers {
    using Microsoft.IIoT.Platform.Publisher;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Publisher message handling
    /// </summary>
    public sealed class DataSetWriterMessageHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.DataSetWriterMessage;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public DataSetWriterMessageHandler(IEnumerable<IDataSetWriterMessageProcessor> handlers,
            IJsonSerializer serializer, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _handlers = handlers?.ToList() ?? throw new ArgumentNullException(nameof(handlers));
        }

        /// <inheritdoc/>
        public async Task HandleAsync(string source, byte[] payload,
            IEventProperties properties, Func<Task> checkpoint) {
            try {
                var sample = _serializer.Deserialize<PublishedDataSetItemMessageModel>(payload);

                //
                // TODO: Decode using binary compressed format directly from payload.
                //

                await Task.WhenAll(_handlers.Select(h => h.HandleMessageAsync(sample))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Exception handling message from {source}", source);
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly List<IDataSetWriterMessageProcessor> _handlers;
    }
}
