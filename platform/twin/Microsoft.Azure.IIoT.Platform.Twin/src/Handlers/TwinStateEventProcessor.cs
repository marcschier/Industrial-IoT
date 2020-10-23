// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Process reported state change messages and update entites in registry
    /// </summary>
    public sealed class TwinStateEventProcessor : ITwinStateProcessor {

        /// <summary>
        /// Create state processor service
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="logger"></param>
        public TwinStateEventProcessor(ITwinStateUpdater registry, ILogger logger) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle state change
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnTwinStateChangeAsync(TwinStateEventModel message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var context = new OperationContextModel {
                Time = message.TimeStamp,
                AuthorityId = null // TODO
            };
            switch (message.EventType) {
                case TwinStateEventType.Connection:
                    if (!string.IsNullOrEmpty(message.TwinId)) {
                        // Patch source state
                        await _registry.UpdateConnectionStateAsync(
                           message.TwinId, message.ConnectionState, context).ConfigureAwait(false);
                        break;
                    }
                    _logger.Warning("Connection event without twin id");
                    break;

                // ...

                default:
                    _logger.Error("Unknown event {eventId}", message.EventType);
                    break;
            }
        }

        private readonly ITwinStateUpdater _registry;
        private readonly ILogger _logger;
    }
}
