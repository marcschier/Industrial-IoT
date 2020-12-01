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
        /// Event hub connection string
        /// </summary>
        public string EventHubConnString { get; set; }

        /// <summary>
        /// Event hub name
        /// </summary>
        public string EventHubPath { get; set; }

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
