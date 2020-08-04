// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.LogAnalytics.Runtime {
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Azure Log Analytics Workspace configuration
    /// </summary>
    public class LogAnalyticsConfig : ConfigBase, ILogAnalyticsConfig {

        private const string kWorkspaceId = "LogAnalytics:WorkspaceId";
        private const string kWorkspaceKey = "LogAnalytics:WorkspaceKey";
        private const string kLogType = "LogAnalytics:LogType";

        /// <inheritdoc/>
        public string LogWorkspaceId => GetStringOrDefault(kWorkspaceId,
            () => GetStringOrDefault(PcsVariable.PCS_WORKSPACE_ID));
        /// <inheritdoc/>
        public string LogWorkspaceKey => GetStringOrDefault(kWorkspaceKey,
            () => GetStringOrDefault(PcsVariable.PCS_WORKSPACE_KEY));
        /// <inheritdoc/>
        public string LogType => GetStringOrDefault(kLogType,
            () => null);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public LogAnalyticsConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
