// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin {
    using Microsoft.IIoT.Platform.Twin.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Browse services via twin
    /// </summary>
    public interface IBrowseServices<T> {

        /// <summary>
        /// Browse nodes on twin
        /// </summary>
        /// <param name="twin">Twin of the server to talk to</param>
        /// <param name="request">Browse request</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseResultModel> NodeBrowseFirstAsync(T twin,
            BrowseRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Browse remainder of references
        /// </summary>
        /// <param name="twin">Twin of the server to talk to</param>
        /// <param name="request">Continuation token</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowseNextResultModel> NodeBrowseNextAsync(T twin,
            BrowseNextRequestModel request, CancellationToken ct = default);

        /// <summary>
        /// Browse by path
        /// </summary>
        /// <param name="twin">Twin of the server to talk to</param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrowsePathResultModel> NodeBrowsePathAsync(T twin,
            BrowsePathRequestModel request, CancellationToken ct = default);
    }
}
