// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.LogAnalytics.Runtime {
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Log Analytics Workspace configuration
    /// </summary>
    public class LogAnalyticsConfig : ConfigBase<LogAnalyticsOptions> {

        /// <inheritdoc/>
        public LogAnalyticsConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, LogAnalyticsOptions options) {
            options.LogWorkspaceId = GetStringOrDefault(PcsVariable.PCS_WORKSPACE_ID);
            options.LogWorkspaceKey = GetStringOrDefault(PcsVariable.PCS_WORKSPACE_KEY);
        }
    }
}
