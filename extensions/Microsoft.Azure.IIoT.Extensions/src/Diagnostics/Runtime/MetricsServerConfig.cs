// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Diagnostics {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Metrics server configuration
    /// </summary>
    public class MetricsServerConfig : ConfigBase<MetricsServerOptions> {

        /// <inheritdoc/>
        public MetricsServerConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, MetricsServerOptions options) {
            options.DiagnosticsLevel = (DiagnosticsLevel)GetIntOrDefault(
                PcsVariable.PCS_DIAGNOSTICS_LEVEL, () => 0);
        }
    }
}
