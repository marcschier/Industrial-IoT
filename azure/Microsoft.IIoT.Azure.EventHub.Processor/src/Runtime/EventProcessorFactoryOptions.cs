// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor {
    using System;

    /// <summary>
    /// Eventprocessor configuration
    /// </summary>
    public class EventProcessorFactoryOptions {

        /// <summary>
        /// Set checkpoint interval. null = never.
        /// </summary>
        public TimeSpan? CheckpointInterval { get; set; }

        /// <summary>
        /// Skip all events older than. null = never.
        /// </summary>
        public TimeSpan? SkipEventsOlderThan { get; set; }
    }
}
