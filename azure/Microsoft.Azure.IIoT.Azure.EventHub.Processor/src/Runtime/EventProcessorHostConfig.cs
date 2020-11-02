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
    internal sealed class EventProcessorHostConfig : ConfigBase<EventProcessorHostOptions> {

        /// <inheritdoc/>
        public override void Configure(string name, EventProcessorHostOptions options) {
            options.ReceiveBatchSize = 999;
            options.ReceiveTimeout = TimeSpan.FromSeconds(5);
        }
    }
}
