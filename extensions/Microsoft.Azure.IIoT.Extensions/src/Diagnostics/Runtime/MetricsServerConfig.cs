// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Metrics server configuration
    /// </summary>
    public class MetricsServerConfig : DiagnosticsConfig, IMetricServerConfig {

        /// <summary>
        /// Configuration keys
        /// </summary>
        private const string kPortKey = "Diagnostics:MetricsServer:Port";
        private const string kPathKey = "Diagnostics:MetricsServer:Path";
        private const string kHttpsKey = "Diagnostics:MetricsServer:UseHttps";

        /// <inheritdoc/>
        public int Port => GetIntOrDefault(kPortKey, () => 0);
        /// <inheritdoc/>
        public string Path => GetStringOrDefault(kPathKey, () => null);
        /// <inheritdoc/>
        public bool UseHttps => GetBoolOrDefault(kHttpsKey, () => false);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public MetricsServerConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
