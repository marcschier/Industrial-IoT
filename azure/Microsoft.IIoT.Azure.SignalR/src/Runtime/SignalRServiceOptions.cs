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
        public string SignalRConnString { get; set; }

        /// <summary>
        /// Whether SignalR is configured to be serverless
        /// </summary>
        public bool SignalRServerLess { get; set; }
    }
}