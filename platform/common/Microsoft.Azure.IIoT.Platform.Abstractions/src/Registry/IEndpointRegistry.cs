// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint registry
    /// </summary>
    public interface IEndpointRegistry {

        /// <summary>
        /// Get all endpoints in paged form
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoListModel> ListEndpointsAsync(string continuation,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Find registration of the supplied endpoint.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoListModel> QueryEndpointsAsync(
            EndpointInfoQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get endpoint registration by identifer.
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoModel> GetEndpointAsync(string endpointId,
            CancellationToken ct = default);

        /// <summary>
        /// Update the endpoint
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="model"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateEndpointAsync(string endpointId,
            EndpointInfoUpdateModel model, CancellationToken ct = default);
    }
}
