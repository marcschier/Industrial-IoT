// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Handlers {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Twin state event handling
    /// </summary>
    public sealed class TwinStateEventHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.TwinEvents;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public TwinStateEventHandler(IEnumerable<ITwinStateProcessor> handlers,
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
            IEventProperties properties, Func<Task> checkpoint) {
            TwinStateEventModel change;
            try {
                change = _serializer.Deserialize<TwinStateEventModel>(payload);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to convert twin state change event message {json}.",
                    Encoding.UTF8.GetString(payload));
                return;
            }
            try {
                await Task.WhenAll(_handlers
                    .Select(h => h.OnTwinStateChangeAsync(change))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Handling twin state event failed with exception - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly List<ITwinStateProcessor> _handlers;
    }
}
