// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System;
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
        /// Kick off onboarding of new server
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RegisterAsync(ServerRegistrationRequestApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Kick off a one time discovery on all supervisors
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DiscoverAsync(DiscoveryRequestApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel a discovery request with a particular id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task CancelAsync(DiscoveryCancelApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Register new application.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationRegistrationResponseApiModel> RegisterAsync(
            ApplicationRegistrationRequestApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// Get application for specified unique application id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationRegistrationApiModel> GetApplicationAsync(
            string applicationId, CancellationToken ct = default);

        /// <summary>
        /// Update an application' properties.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all Application sites to visually group applications.
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationSiteListApiModel> ListSitesAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// List all applications or continue a QueryApplications
        /// call.
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoListApiModel> ListApplicationsAsync(
            string continuation = null, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Find applications based on specified criteria. Pass
        /// continuation token if any returned to ListApplications to
        /// retrieve remaining items.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoListApiModel> QueryApplicationsAsync(
            ApplicationRegistrationQueryApiModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Unregister and delete application and all endpoints.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UnregisterApplicationAsync(string applicationId,
            string generationId, CancellationToken ct = default);

        /// <summary>
        /// Unregister disabled applications not seen since specified
        /// amount of time.
        /// </summary>
        /// <param name="notSeenSince"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PurgeDisabledApplicationsAsync(TimeSpan notSeenSince,
            CancellationToken ct = default);

        /// <summary>
        /// Get endpoint
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoApiModel> GetEndpointAsync(
            string endpointId, CancellationToken ct = default);

        /// <summary>
        /// Get endpoint certificate
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<X509CertificateChainApiModel> GetEndpointCertificateAsync(
            string endpointId, CancellationToken ct = default);

        /// <summary>
        /// Update endpoint information including activation state
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateEndpointAsync(string endpointId,
            EndpointInfoUpdateApiModel request,
            CancellationToken ct = default);

        /// <summary>
        /// List all endpoints
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoListApiModel> ListEndpointsAsync(
            string continuation = null,
            int? pageSize = null, CancellationToken ct = default);

        /// <summary>
        /// Find endpoint based on specified criteria. Pass continuation
        /// token if any is returned to ListEndpointsAsync to retrieve
        /// the remaining items
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoListApiModel> QueryEndpointsAsync(
            EndpointInfoQueryApiModel query, int? pageSize = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get supervisor runtime status
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="ct"></param>
        /// <returns>Supervisor diagnostics</returns>
        Task<SupervisorStatusApiModel> GetSupervisorStatusAsync(
            string supervisorId, CancellationToken ct = default);

        /// <summary>
        /// Reset and restart supervisor
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="ct"></param>
        Task ResetSupervisorAsync(string supervisorId,
            CancellationToken ct = default);

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
        /// Enable or disable discovery with optional configuration
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="mode"></param>
        /// <param name="config"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SetDiscoveryModeAsync(string discovererId,
            DiscoveryMode mode, DiscoveryConfigApiModel config = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get publisher runtime status
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="ct"></param>
        /// <returns>Supervisor diagnostics</returns>
        Task<SupervisorStatusApiModel> GetPublisherStatusAsync(
            string publisherId, CancellationToken ct = default);

        /// <summary>
        /// Reset and restart publisher module
        /// </summary>
        /// <param name="publisherId"></param>
        /// <param name="ct"></param>
        Task ResetPublisherAsync(string publisherId,
            CancellationToken ct = default);

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
    }
}
