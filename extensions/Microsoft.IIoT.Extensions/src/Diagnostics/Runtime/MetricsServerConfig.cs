// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Diagnostics {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Metrics server configuration
    /// </summary>
    public class MetricsServerConfig : PostConfigureOptionBase<MetricsServerOptions> {

        /// <inheritdoc/>
        public MetricsServerConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, MetricsServerOptions options) {
            if (options.DiagnosticsLevel == 0) {
                options.DiagnosticsLevel = (DiagnosticsLevel)GetIntOrDefault(
                    PcsVariable.PCS_DIAGNOSTICS_LEVEL);
            }
        }
    }
}
