// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Clients {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Tasks;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Reporter to update adapter
    /// </summary>
    public sealed class DataSetWriterStateAdapter : IDataSetWriterStateReporter {

        /// <summary>
        /// Create listener
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="processor"></param>
        /// <param name="logger"></param>
        public DataSetWriterStateAdapter(IDataSetWriterStateUpdater registry,
            ITaskProcessor processor, ILogger logger) {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _logger = new DataSetWriterStateLogger(logger);
        }

        /// <inheritdoc/>
        public void OnDataSetEventStateChange(string dataSetWriterId,
            PublishedDataSetItemStateModel state) {
            _logger.OnDataSetEventStateChange(dataSetWriterId, state);
            _processor.TrySchedule(() => _registry.UpdateDataSetEventStateAsync(
                dataSetWriterId, state));
        }

        /// <inheritdoc/>
        public void OnDataSetVariableStateChange(string dataSetWriterId,
            string variableId, PublishedDataSetItemStateModel state) {
            _logger.OnDataSetVariableStateChange(dataSetWriterId, variableId, state);
            _processor.TrySchedule(() => _registry.UpdateDataSetVariableStateAsync(
                dataSetWriterId, variableId, state));
        }

        /// <inheritdoc/>
        public void OnDataSetWriterStateChange(string dataSetWriterId,
            PublishedDataSetSourceStateModel state) {
            _logger.OnDataSetWriterStateChange(dataSetWriterId, state);
            _processor.TrySchedule(() => _registry.UpdateDataSetWriterStateAsync(
                dataSetWriterId, state));
        }

        private readonly DataSetWriterStateLogger _logger;
        private readonly IDataSetWriterStateUpdater _registry;
        private readonly ITaskProcessor _processor;
    }
}