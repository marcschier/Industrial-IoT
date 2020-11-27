// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Handlers {
    using Microsoft.Azure.IIoT.Platform.Discovery.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Discovery message handling
    /// </summary>
    public sealed class DiscoveryProgressEventHandler : ITelemetryHandler {

        /// <inheritdoc/>
        public string MessageSchema => MessageSchemaTypes.DiscoveryMessage;

        /// <summary>
        /// Create handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public DiscoveryProgressEventHandler(IEnumerable<IDiscovererProgressProcessor> handlers,
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
            DiscoveryProgressModel discovery;
            try {
                discovery = _serializer.Deserialize<DiscoveryProgressModel>(payload);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to convert discovery message {json}",
                    Encoding.UTF8.GetString(payload));
                return;
            }
            try {
                await Task.WhenAll(_handlers.Select(h => h.OnDiscoveryProgressAsync(discovery))).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.LogError(ex,
                    "Publishing discovery message failed with exception - skip");
            }
        }

        /// <inheritdoc/>
        public Task OnBatchCompleteAsync() {
            return Task.CompletedTask;
        }

        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
        private readonly List<IDiscovererProgressProcessor> _handlers;
    }
}
