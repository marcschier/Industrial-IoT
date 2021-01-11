// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Event bus subscriber
    /// </summary>
    public interface IEventBusSubscriber {

        /// <summary>
        /// Register handler
        /// </summary>
        /// <param name="handler"></param>
        Task<IAsyncDisposable> SubscribeAsync<T>(IEventBusConsumer<T> handler);
    }
}
