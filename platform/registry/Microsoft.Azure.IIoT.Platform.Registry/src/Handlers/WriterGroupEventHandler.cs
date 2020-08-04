// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Handlers {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
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
        public string MessageSchema => Models.MessageSchemaTypes.WriterGroupEvents;

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
        public async Task HandleAsync(string deviceId, string moduleId, byte[] payload,
            IDictionary<string, string> properties, Func<Task> checkpoint) {
            WriterGroupStateEventModel change;
            try {
                change = _serializer.Deserialize<WriterGroupStateEventModel>(payload);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to convert publisher state change event message {json}.",
                    Encoding.UTF8.GetString(payload));
                return;
            }
            try {
                await Task.WhenAll(_handlers.Select(h => h.OnWriterGroupStateChangeAsync(change)));
            }
            catch (Exception ex) {
                _logger.Error(ex, "Handling publisher state event failed with exception - skip");
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
