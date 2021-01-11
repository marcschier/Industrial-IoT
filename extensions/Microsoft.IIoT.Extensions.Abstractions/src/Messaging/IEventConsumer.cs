// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles events
    /// </summary>
    public interface IEventConsumer : IHandler {

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="properties"></param>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        Task HandleAsync(byte[] eventData,
            IEventProperties properties, Func<Task> checkpoint);

        /// <summary>
        /// Event batch completed
        /// </summary>
        /// <returns></returns>
        Task OnBatchCompleteAsync();
    }
}
