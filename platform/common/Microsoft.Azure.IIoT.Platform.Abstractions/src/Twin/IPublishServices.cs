// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Publish services
    /// </summary>
    public interface IPublishServices {

        /// <summary>
        /// Start publishing node values
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishStartResultModel> NodePublishStartAsync(string endpointId,
            PublishStartRequestModel request);

        /// <summary>
        /// Start publishing node values
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishStopResultModel> NodePublishStopAsync(string endpointId,
            PublishStopRequestModel request);

        /// <summary>
        /// Configure nodes to publish and unpublish in bulk
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishBulkResultModel> NodePublishBulkAsync(string endpointId,
            PublishBulkRequestModel request);

        /// <summary>
        /// Get all published nodes for endpoint.
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishedItemListResultModel> NodePublishListAsync(
            string endpointId, PublishedItemListRequestModel request);
    }
}
