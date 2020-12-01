// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Handlers {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Messaging;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Writer group state event handling
    /// </summary>
    public sealed class WriterGroupEventHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.WriterGroupEvents;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public WriterGroupEventHandler(IEnumerable<IWriterGroupStateProcessor> handlers,
            IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _handlers = handlers?.ToList() ??
                throw new ArgumentNullException(nameof(handlers));
        }

        /// <inheritdoc/>
        public async Task HandleAsync(string source, byte[] payload,
            IDictionary<string, string> properties, Func<Task> checkpoint) {
            WriterGroupStateEventModel change;
            try {
                change = _serializer.Deserialize<WriterGroupStateEventModel>(payload);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to convert writer group state change event message {json}.",
                    Encoding.UTF8.GetString(payload));
                return;
            }
            try {
                await Task.WhenAll(_handlers.Select(h => h.OnWriterGroupStateChangeAsync(
                    change))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Handling writer group state event failed with exception - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly List<IWriterGroupStateProcessor> _handlers;
    }
}
