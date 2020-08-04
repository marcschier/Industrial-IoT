// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.LogAnalytics {

    /// <summary>
    /// Azure Log Analytics Workspace configuration
    /// </summary>
    public interface ILogAnalyticsConfig {

        /// <summary>
        /// Log Analytics Workspace Id
        /// </summary>
        string LogWorkspaceId { get; }

        /// <summary>
        /// Log Analytics Workspace Key
        /// </summary>
        string LogWorkspaceKey { get; }

        /// <summary>
        /// Log Analytics table
        /// </summary>
        string LogType { get; }
    }
}