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
    internal sealed class EventProcessorHostConfig : PostConfigureOptionBase<EventProcessorHostOptions> {

        /// <inheritdoc/>
        public override void PostConfigure(string name, EventProcessorHostOptions options) {
            if (options.ReceiveBatchSize <= 0) {
                options.ReceiveBatchSize = 50;
            }
            if (options.ReceiveTimeout == TimeSpan.Zero) {
                options.ReceiveTimeout = TimeSpan.FromSeconds(5);
            }
        }
    }
}
