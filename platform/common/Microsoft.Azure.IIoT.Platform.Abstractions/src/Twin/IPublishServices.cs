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
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishStartResultModel> NodePublishStartAsync(string twinId,
            PublishStartRequestModel request);

        /// <summary>
        /// Start publishing node values
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishStopResultModel> NodePublishStopAsync(string twinId,
            PublishStopRequestModel request);

        /// <summary>
        /// Configure nodes to publish and unpublish in bulk
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishBulkResultModel> NodePublishBulkAsync(string twinId,
            PublishBulkRequestModel request);

        /// <summary>
        /// Get all published nodes for twin.
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PublishedItemListResultModel> NodePublishListAsync(
            string twinId, PublishedItemListRequestModel request);
    }
}
