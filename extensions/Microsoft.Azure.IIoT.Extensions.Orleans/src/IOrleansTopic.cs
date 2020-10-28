// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans {
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
        /// <param name="sub"></param>
        /// <returns></returns>
        Task SubscribeAsync(IOrleansSubscription sub);

        /// <summary>
        /// Unsubscribe from topic
        /// </summary>
        /// <param name="sub"></param>
        /// <returns></returns>
        Task UnsubscribeAsync(IOrleansSubscription sub);
    }
}