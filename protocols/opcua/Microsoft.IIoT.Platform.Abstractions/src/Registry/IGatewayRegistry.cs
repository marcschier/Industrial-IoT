// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry {
    using Microsoft.IIoT.Platform.Registry.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Edge Gateway registry
    /// </summary>
    public interface IGatewayRegistry {

        /// <summary>
        /// Get all gateways in paged form
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayListModel> ListGatewaysAsync(
            string continuation, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find gateways using specific criterias.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayListModel> QueryGatewaysAsync(
            GatewayQueryModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get gateway registration by identifer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayInfoModel> GetGatewayAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Update gateway
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateGatewayAsync(string id, GatewayUpdateModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List gateway sites
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewaySiteListModel> ListSitesAsync(
            string continuation, int? pageSize,
            CancellationToken ct = default);
    }
}