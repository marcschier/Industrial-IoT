// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.AppInsights.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// App Insights configuration
    /// </summary>
    internal sealed class AppInsightsConfig : PostConfigureOptionBase<AppInsightsOptions> {

        /// <inheritdoc/>
        public AppInsightsConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, AppInsightsOptions options) {
            if (string.IsNullOrEmpty(options.InstrumentationKey)) {
                options.InstrumentationKey =
                    GetStringOrDefault(PcsVariable.PCS_APPINSIGHTS_INSTRUMENTATIONKEY);
            }
        }
    }
}
