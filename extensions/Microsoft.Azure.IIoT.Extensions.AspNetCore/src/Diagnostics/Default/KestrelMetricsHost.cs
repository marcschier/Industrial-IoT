// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Diagnostics.Default {
    using Microsoft.Azure.IIoT.Diagnostics.Services;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Start and stop metric server
    /// </summary>
    public class KestrelMetricsHost : MetricsHost {

        /// <inheritdoc/>
        public KestrelMetricsHost(IEnumerable<IMetricsHandler> handlers, ILogger logger,
            IMetricServerConfig config = null) : base(handlers, logger, config) {
        }

        /// <inheritdoc/>
        protected override IMetricServer CreateServer(IMetricServerConfig config) {
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            return new KestrelMetricServer(config.Port, config.Path ?? "/metrics");
        }
    }
}
