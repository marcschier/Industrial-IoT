// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.LogAnalytics.Runtime {
    using Microsoft.IIoT.Azure.LogAnalytics;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Log Analytics Workspace configuration
    /// </summary>
    public class LogAnalyticsConfig : PostConfigureOptionBase<LogAnalyticsOptions> {

        /// <inheritdoc/>
        public LogAnalyticsConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, LogAnalyticsOptions options) {
            if (string.IsNullOrEmpty(options.LogWorkspaceId)) {
                options.LogWorkspaceId = GetStringOrDefault(PcsVariable.PCS_WORKSPACE_ID);
            }
            if (string.IsNullOrEmpty(options.LogWorkspaceKey)) {
                options.LogWorkspaceKey = GetStringOrDefault(PcsVariable.PCS_WORKSPACE_KEY);
            }
        }
    }
}
