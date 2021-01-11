// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor {

    /// <summary>
    /// Event hub configuration
    /// </summary>
    public class EventHubConsumerOptions {

        /// <summary>
        /// Event hub namespace connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Event hub path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether to use websockets
        /// </summary>
        public bool UseWebsockets { get; set; }

        /// <summary>
        /// Consumer group
        /// (optional, default to $default)
        /// </summary>
        public string ConsumerGroup { get; set; }
    }
}
