// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Diagnostics {
    using System;

    /// <summary>
    /// Configuration for metric server
    /// </summary>
    public class MetricsServerOptions {

        /// <summary>
        /// Level of diagnostics
        /// </summary>
        public DiagnosticsLevel DiagnosticsLevel { get; set; }

        /// <summary>
        /// Metrics collection interval if configured
        /// </summary>
        public TimeSpan? MetricsCollectionInterval { get; set; }

        /// <summary>
        /// Metric server port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Path to expose
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Use https
        /// </summary>
        public bool UseHttps { get; set; }
    }
}
