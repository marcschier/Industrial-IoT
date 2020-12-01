// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.AspNetCore.Diagnostics.Default {
    using Microsoft.IIoT.Diagnostics.Services;
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Prometheus;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Start and stop metric server
    /// </summary>
    public class KestrelMetricsHost : MetricsHost {

        /// <inheritdoc/>
        public KestrelMetricsHost(IEnumerable<IMetricsHandler> handlers, ILogger logger,
            IOptions<MetricsServerOptions> options) : base(handlers, logger, options) {
        }

        /// <inheritdoc/>
        protected override IMetricServer CreateServer(MetricsServerOptions options) {
            if (options is null) {
                throw new ArgumentNullException(nameof(options));
            }
            return new KestrelMetricServer(options.Port, options.Path ?? "/metrics");
        }
    }
}
