// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Handlers {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Writer state event handling
    /// </summary>
    public sealed class ConnectionEventHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.EndpointEvents;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public ConnectionEventHandler(IEnumerable<IConnectionStateProcessor> handlers,
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
            ConnectionStateEventModel change;
            try {
                change = _serializer.Deserialize<ConnectionStateEventModel>(payload);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to convert endpoint state change event message {json}.",
                    Encoding.UTF8.GetString(payload));
                return;
            }
            try {
                await Task.WhenAll(_handlers.Select(h => h.OnConnectionStateChangeAsync(
                    change))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Handling endpoint state event failed with exception - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly List<IConnectionStateProcessor> _handlers;
    }
}
