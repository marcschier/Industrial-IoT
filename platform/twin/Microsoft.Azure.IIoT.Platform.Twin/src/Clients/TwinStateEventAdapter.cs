// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Clients {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Reporter to update adapter
    /// </summary>
    public sealed class TwinStateEventAdapter : ITwinStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="processor"></param>
        /// <param name="logger"></param>
        public TwinStateEventAdapter(ITwinStateUpdater registry,
            ITaskProcessor processor, ILogger logger) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _logger = new TwinStateEventLogger(logger);
        }

        /// <inheritdoc/>
        public void OnConnectionStateChange(string twinId, ConnectionStateModel state) {
            _logger.OnConnectionStateChange(twinId, state);
            _processor.TrySchedule(() => _registry.UpdateConnectionStateAsync(twinId, state));
        }

        private readonly TwinStateEventLogger _logger;
        private readonly ITwinStateUpdater _registry;
        private readonly ITaskProcessor _processor;
    }
}