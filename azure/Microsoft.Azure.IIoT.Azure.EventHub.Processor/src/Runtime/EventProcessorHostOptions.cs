// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor {
    using System;

    /// <summary>
    /// Eventprocessor host configuration
    /// </summary>
    public class EventProcessorHostOptions : StorageOptions {

        /// <summary>
        /// Receive batch size
        /// </summary>
        public int ReceiveBatchSize { get; set; }

        /// <summary>
        /// Receive timeout
        /// </summary>
        public TimeSpan ReceiveTimeout { get; set; }

        /// <summary>
        /// Whether to read from end or start.
        /// </summary>
        public bool InitialReadFromEnd { get; set; }

        /// <summary>
        /// And lease container name.
        /// </summary>
        public string LeaseContainerName { get; set; }
    }
}
