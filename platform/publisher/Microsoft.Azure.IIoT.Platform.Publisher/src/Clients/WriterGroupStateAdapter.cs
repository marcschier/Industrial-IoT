// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Clients {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Tasks;
    using Serilog;
    using System;

    /// <summary>
    /// Reporter to update adapter
    /// </summary>
    public sealed class WriterGroupStateAdapter : IWriterGroupStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="processor"></param>
        /// <param name="logger"></param>
        public WriterGroupStateAdapter(IWriterGroupStateUpdater registry,
            ITaskProcessor processor, ILogger logger) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <inheritdoc/>
        public void OnWriterGroupStateChange(string writerGroupId, WriterGroupStatus? state) {
            _logger.Information("{writerGroup} changed state to {state}", 
                writerGroupId, state);
            _processor.TrySchedule(() => _registry.UpdateWriterGroupStateAsync(
                writerGroupId, state));
        }


        private readonly ILogger _logger;
        private readonly IWriterGroupStateUpdater _registry;
        private readonly ITaskProcessor _processor;
    }
}