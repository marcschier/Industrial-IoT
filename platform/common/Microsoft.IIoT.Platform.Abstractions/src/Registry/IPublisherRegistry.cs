// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry {
    using Microsoft.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher registry
    /// </summary>
    public interface IPublisherRegistry {

        /// <summary>
        /// Get all publishers in paged form
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherListModel> ListPublishersAsync(
            string continuation, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find publishers using specific criterias.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherListModel> QueryPublishersAsync(
            PublisherQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get publisher registration by identifer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherModel> GetPublisherAsync(
            string id, CancellationToken ct = default);

        /// <summary>
        /// Update publisher configuration
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdatePublisherAsync(string id,
            PublisherUpdateModel request,
            CancellationToken ct = default);
    }
}
