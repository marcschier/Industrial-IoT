// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Edge.Clients {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Serilog;
    using System;

    /// <summary>
    /// Logs state events
    /// </summary>
    public sealed class WriterGroupStateLogger : IWriterGroupStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="logger"></param>
        public WriterGroupStateLogger(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void OnDataSetEventStateChange(string dataSetWriterId,
            PublishedDataSetItemStateModel state) {
            _logger.Debug("Event definition state for {dataSetWriterId} changed to {@state}",
                dataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnDataSetVariableStateChange(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state) {
            _logger.Debug("Variable {variableId} in {dataSetWriterId} changed to {@state}",
                variableId, dataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnDataSetWriterStateChange(string dataSetWriterId,
            PublishedDataSetSourceStateModel state) {
            _logger.Debug("Data Set writer {dataSetWriterId} stat changed {@state}",
                dataSetWriterId, state);
        }

        private readonly ILogger _logger;
    }
}