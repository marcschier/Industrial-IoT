// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa {
    using Microsoft.IIoT.Platform.OpcUa.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Subscription manager
    /// </summary>
    public interface ISubscriptionClient {

        /// <summary>
        /// Total subscriptions
        /// </summary>
        int TotalSubscriptionCount { get; }

        /// <summary>
        /// Get or create new subscription
        /// </summary>
        /// <param name="subscriptionModel"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        Task<ISubscriptionHandle> CreateSubscriptionAsync(
            SubscriptionModel subscriptionModel, 
            ISubscriptionListener listener);
    }
}