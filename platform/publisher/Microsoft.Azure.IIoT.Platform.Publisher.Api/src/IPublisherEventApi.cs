// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher Event controller api
    /// </summary>
    public interface IPublisherEventApi {

        /// <summary>
        /// Subscribe to dataset item status changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string userId, CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe from dataset item status changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnsubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string userId, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to dataset writer messages
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SubscribeDataSetWriterMessagesAsync(string dataSetWriterId,
            string userId, CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe from receiving dataset writer messages
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnsubscribeDataSetWriterMessagesAsync(string dataSetWriterId,
            string userId, CancellationToken ct = default);

        /// <summary>
        /// Subscribe client to receive published samples
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task NodePublishSubscribeByEndpointAsync(string endpointId,
            string userId, CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe client from receiving samples
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task NodePublishUnsubscribeByEndpointAsync(string endpointId,
            string userId, CancellationToken ct = default);
    }
}
