// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api {
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry api calls
    /// </summary>
    public interface IRegistryServiceApi {

        /// <summary>
        /// Returns status of the service
        /// </summary>
        /// <returns></returns>
        Task<string> GetServiceStatusAsync(CancellationToken ct = default);

        /// <summary>
        /// Get supervisor
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<SupervisorApiModel> GetSupervisorAsync(
            string supervisorId, CancellationToken ct = default);

        /// <summary>
        /// Update supervisor including config updates.
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateSupervisorAsync(string supervisorId,
            SupervisorUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all supervisors
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<SupervisorListApiModel> ListSupervisorsAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find supervisors based on specified criteria. Pass
        /// continuation token if any returned to ListSupervisors to
        /// retrieve remaining items.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<SupervisorListApiModel> QuerySupervisorsAsync(
            SupervisorQueryApiModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get discoverer
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DiscovererApiModel> GetDiscovererAsync(
            string discovererId, CancellationToken ct = default);

        /// <summary>
        /// Update discoverer including config updates.
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateDiscovererAsync(string discovererId,
            DiscovererUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all discoverers
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DiscovererListApiModel> ListDiscoverersAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find discoverers based on specified criteria. Pass
        /// continuation token if any returned to ListDiscoverers to
        /// retrieve remaining items.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DiscovererListApiModel> QueryDiscoverersAsync(
            DiscovererQueryApiModel query,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get gateway
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherApiModel> GetPublisherAsync(
            string publisherId,
            CancellationToken ct = default);

        /// <summary>
        /// Update Publisher including config updates.
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdatePublisherAsync(string publisherId,
            PublisherUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all gateways
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherListApiModel> ListPublishersAsync(
            string continuation = null,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Find gateways based on specified criteria. Pass
        /// continuation token if any returned to ListPublishers to
        /// retrieve remaining items.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PublisherListApiModel> QueryPublishersAsync(
            PublisherQueryApiModel query,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Get gateway
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayInfoApiModel> GetGatewayAsync(
            string gatewayId, CancellationToken ct = default);

        /// <summary>
        /// Update Gateway including config updates.
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateGatewayAsync(string gatewayId,
            GatewayUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all gateways
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayListApiModel> ListGatewaysAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find gateways based on specified criteria. Pass
        /// continuation token if any returned to ListGateways to
        /// retrieve remaining items.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewayListApiModel> QueryGatewaysAsync(
            GatewayQueryApiModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// List all sites to visually group gateways.
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<GatewaySiteListApiModel> ListSitesAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);
    }
}
