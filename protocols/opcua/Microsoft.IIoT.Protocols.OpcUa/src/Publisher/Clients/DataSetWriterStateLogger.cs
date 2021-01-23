// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Logs state events
    /// </summary>
    public sealed class DataSetWriterStateLogger : IDataSetWriterStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="logger"></param>
        public DataSetWriterStateLogger(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void OnDataSetEventStateChange(string dataSetWriterId,
            PublishedDataSetItemStateModel state) {
            _logger.LogDebug("Event definition state for {dataSetWriterId} changed to {@state}",
                dataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnDataSetVariableStateChange(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state) {
            _logger.LogDebug("Variable {variableId} in {dataSetWriterId} changed to {@state}",
                variableId, dataSetWriterId, state);
        }

        /// <inheritdoc/>
        public void OnDataSetWriterStateChange(string dataSetWriterId,
            PublishedDataSetSourceStateModel state) {
            _logger.LogDebug("Data Set writer {dataSetWriterId} stat changed {@state}",
                dataSetWriterId, state);
        }

        private readonly ILogger _logger;
    }
}