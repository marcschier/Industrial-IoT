// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
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
    }
}
