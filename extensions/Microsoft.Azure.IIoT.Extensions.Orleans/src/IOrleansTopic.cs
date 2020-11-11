﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans {
    using System.Threading.Tasks;
    using global::Orleans;

    /// <summary>
    /// Topic
    /// </summary>
    public interface IOrleansTopic : IGrainWithStringKey {

        /// <summary>
        /// Publish to topic
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        Task PublishAsync(byte[] buffer);

        /// <summary>
        /// Subscribe to topic
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task SubscribeAsync(IOrleansSubscription subscription);

        /// <summary>
        /// Unsubscribe from topic
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task UnsubscribeAsync(IOrleansSubscription subscription);
    }
}