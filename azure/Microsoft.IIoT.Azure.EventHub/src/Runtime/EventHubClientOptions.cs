// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub {

    /// <summary>
    /// Event hub configuration
    /// </summary>
    public class EventHubClientOptions {

        /// <summary>
        /// Event hub namespace connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Event hub path
        /// </summary>
        public string Path { get; set; }
    }
}
