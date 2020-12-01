// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Messaging {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Subscriptions to target events
    /// </summary>
    public interface IEventSubscriberClient {

        /// <summary>
        /// Subscribe to target and consume.  Subscriptions are
        /// transient and exist only as long as the process runs.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="consumer"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeAsync(string target,
            IEventConsumer consumer);
    }
}
