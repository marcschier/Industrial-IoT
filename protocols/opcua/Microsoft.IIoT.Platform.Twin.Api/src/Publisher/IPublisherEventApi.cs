// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api {
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
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe from dataset item status changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnsubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct = default);

        /// <summary>
        /// Subscribe client to receive published samples
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SubscribeDataSetVariableMessagesAsync(string dataSetWriterId,
            string variableId, string connectionId,
            CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe client from receiving samples
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnsubscribeDataSetVariableMessagesAsync(string dataSetWriterId,
            string variableId, string connectionId,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to event changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SubscribeDataSetEventMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct = default);

        /// <summary>
        /// Unsubscribe client from receiving events
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="connectionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnsubscribeDataSetEventMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct = default);
    }
}
