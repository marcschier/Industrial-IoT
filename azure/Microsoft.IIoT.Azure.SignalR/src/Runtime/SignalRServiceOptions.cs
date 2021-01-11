// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.SignalR {
    /// <summary>
    /// SignalR service configuration
    /// </summary>
    public class SignalRServiceOptions {

        /// <summary>
        /// SignalR connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Whether SignalR is configured to be serverless
        /// </summary>
        public bool IsServerLess { get; set; }
    }
}