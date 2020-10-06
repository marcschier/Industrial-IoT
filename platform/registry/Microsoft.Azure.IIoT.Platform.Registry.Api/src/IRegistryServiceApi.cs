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
    }
}
