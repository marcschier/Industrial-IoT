// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles events
    /// </summary>
    public interface ITelemetryHandler : IHandler {

        /// <summary>
        /// Event message schema
        /// </summary>
        string MessageSchema { get; }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="source"></param>
        /// <param name="payload"></param>
        /// <param name="properties"></param>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        Task HandleAsync(string source, byte[] payload,
            IEventProperties properties, Func<Task> checkpoint);

        /// <summary>
        /// Called when batch is completed
        /// </summary>
        /// <returns></returns>
        Task OnBatchCompleteAsync();
    }
}
