// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.IIoT.Configuration;
    using System;

    /// <summary>
    /// Event processor configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventProcessorFactoryConfig : PostConfigureOptionBase<EventProcessorFactoryOptions> {

        /// <inheritdoc/>
        public override void PostConfigure(string name, EventProcessorFactoryOptions options) {
#if DEBUG
            if (options.SkipEventsOlderThan == null) {
                options.SkipEventsOlderThan = TimeSpan.FromMinutes(5);
            }
#endif
            if (options.CheckpointInterval == null) {
                options.CheckpointInterval = TimeSpan.FromMinutes(1);
            }
        }
    }
}
