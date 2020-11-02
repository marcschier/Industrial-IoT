// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.AppInsights.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// App Insights configuration
    /// </summary>
    internal sealed class AppInsightsConfig : ConfigBase<AppInsightsOptions> {

        /// <inheritdoc/>
        public AppInsightsConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, AppInsightsOptions options) {
            options.InstrumentationKey = 
                GetStringOrDefault(PcsVariable.PCS_APPINSIGHTS_INSTRUMENTATIONKEY,
                    () => null);
        }
    }
}
