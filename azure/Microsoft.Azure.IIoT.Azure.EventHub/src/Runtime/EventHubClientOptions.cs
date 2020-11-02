// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub {

    /// <summary>
    /// Event hub configuration
    /// </summary>
    public class EventHubClientOptions {

        /// <summary>
        /// Event hub connection string
        /// </summary>
        public string EventHubConnString { get; set; }

        /// <summary>
        /// Event hub name
        /// </summary>
        public string EventHubPath { get; set; }
    }
}
