// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using System;

    /// <summary>
    /// Event processor configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventProcessorFactoryConfig : ConfigBase<EventProcessorFactoryOptions> {

        /// <inheritdoc/>
        public override void Configure(string name, EventProcessorFactoryOptions options) {
#if DEBUG
            options.SkipEventsOlderThan = TimeSpan.FromMinutes(5);
#endif
            options.CheckpointInterval = TimeSpan.FromMinutes(1);
        }
    }
}
