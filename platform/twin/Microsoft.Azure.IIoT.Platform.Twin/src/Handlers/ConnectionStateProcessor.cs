// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Services {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Process reported state change messages and update entites in registry
    /// </summary>
    public sealed class ConnectionStateProcessor : IConnectionStateProcessor {

        /// <summary>
        /// Create state processor service
        /// </summary>
        /// <param name="registry"></param>
        public ConnectionStateProcessor(IConnectionStateUpdater registry) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Handle state change
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnConnectionStateChangeAsync(ConnectionStateEventModel message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var context = new OperationContextModel {
                Time = message.State?.LastResultChange ?? DateTime.UtcNow,
                AuthorityId = null // TODO
            };
            if (!string.IsNullOrEmpty(message.ConnectionId)) {
                // Patch source state
                await _registry.UpdateConnectionStateAsync(
                   message.ConnectionId, message.State, context).ConfigureAwait(false);
            }
        }

        private readonly IConnectionStateUpdater _registry;
    }
}
