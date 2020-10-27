// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans {
    using global::Orleans;

    /// <summary>
    /// Subscription
    /// </summary>
    public interface IOrleansSubscription : IGrainObserver {

        /// <summary>
        /// Receive buffer from topic
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        void Consume(byte[] buffer);
    }
}