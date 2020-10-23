// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Logs state events
    /// </summary>
    public sealed class TwinStateEventLogger : ITwinStateReporter {

        /// <summary>
        /// Create logger
        /// </summary>
        /// <param name="logger"></param>
        public TwinStateEventLogger(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void OnConnectionStateChange(string twinId, ConnectionStateModel state) {
            _logger.LogDebug("Twin writer {twinId} connection state changed to {@state}",
                twinId, state);
        }

        private readonly ILogger _logger;
    }
}