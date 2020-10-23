// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Clients {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Adapts discovery result handling onto bulk processor for in process handling
    /// </summary>
    public sealed class DiscoveryBulkProcessorAdapter : IDiscoveryResultHandler {

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="identity"></param>
        /// <param name="logger"></param>
        public DiscoveryBulkProcessorAdapter(IApplicationBulkProcessor processor,
            IIdentity identity, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        /// <inheritdoc/>
        public async Task ReportResultsAsync(IEnumerable<DiscoveryResultModel> results,
            CancellationToken ct) {
            try {
                await _processor.ProcessDiscoveryEventsAsync(_identity.AsHubResource(),
                    results.LastOrDefault(r => r.Result != null).Result,
                    results.Where(r => r.Application != null)).ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to process discovery result");
            }
        }

        private readonly ILogger _logger;
        private readonly IApplicationBulkProcessor _processor;
        private readonly IIdentity _identity;
    }
}
