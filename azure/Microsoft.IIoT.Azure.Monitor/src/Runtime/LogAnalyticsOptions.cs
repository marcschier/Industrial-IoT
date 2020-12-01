// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.LogAnalytics {
    /// <summary>
    /// Azure Log Analytics Workspace configuration
    /// </summary>
    public class LogAnalyticsOptions {

        /// <summary>
        /// Log Analytics Workspace Id
        /// </summary>
        public string LogWorkspaceId { get; set; }

        /// <summary>
        /// Log Analytics Workspace Key
        /// </summary>
        public string LogWorkspaceKey { get; set; }

        /// <summary>
        /// Log Analytics table
        /// </summary>
        public string LogType { get; set; }
    }
}