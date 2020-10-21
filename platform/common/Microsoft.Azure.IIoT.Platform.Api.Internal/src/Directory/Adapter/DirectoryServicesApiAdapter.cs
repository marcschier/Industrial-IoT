// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Directory;
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Directory services adapter to run dependent services outside of cloud.
    /// </summary>
    public sealed class DirectoryServicesApiAdapter : ISupervisorRegistry, 
        IPublisherRegistry, IDiscovererRegistry, IGatewayRegistry {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="client"></param>
        public DirectoryServicesApiAdapter(IDirectoryServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<SupervisorListModel> ListSupervisorsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListSupervisorsAsync(continuation,
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<SupervisorListModel> QuerySupervisorsAsync(
            SupervisorQueryModel query, int? pageSize,
            CancellationToken ct) {
            var result = await _client.QuerySupervisorsAsync(query.ToApiModel(),
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<SupervisorModel> GetSupervisorAsync(string id,
            CancellationToken ct) {
            var result = await _client.GetSupervisorAsync(id, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdateSupervisorAsync(string supervisorId,
            SupervisorUpdateModel request, CancellationToken ct) {
            await _client.UpdateSupervisorAsync(supervisorId, request.ToApiModel(), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> ListPublishersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListPublishersAsync(continuation,
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublisherListModel> QueryPublishersAsync(
            PublisherQueryModel query, int? pageSize,
            CancellationToken ct) {
            var result = await _client.QueryPublishersAsync(query.ToApiModel(),
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<PublisherModel> GetPublisherAsync(string id,
            CancellationToken ct) {
            var result = await _client.GetPublisherAsync(id, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdatePublisherAsync(string id, 
            PublisherUpdateModel request, CancellationToken ct) {
            await _client.UpdatePublisherAsync(id, request.ToApiModel(), 
                ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<GatewaySiteListModel> ListSitesAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListSitesAsync(continuation, pageSize,
                ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<GatewayListModel> ListGatewaysAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListGatewaysAsync(continuation, 
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<GatewayListModel> QueryGatewaysAsync(
            GatewayQueryModel query, int? pageSize, CancellationToken ct) {
            var result = await _client.QueryGatewaysAsync(
                query.ToApiModel(), pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<GatewayInfoModel> GetGatewayAsync(
            string id, CancellationToken ct) {
            var result = await _client.GetGatewayAsync(id, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdateGatewayAsync(string id, 
            GatewayUpdateModel request, CancellationToken ct) {
            await _client.UpdateGatewayAsync(id, request.ToApiModel(), 
                ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<DiscovererListModel> ListDiscoverersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListDiscoverersAsync(
                continuation, pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<DiscovererListModel> QueryDiscoverersAsync(
            DiscovererQueryModel query, int? pageSize, CancellationToken ct) {
            var result = await _client.QueryDiscoverersAsync(
                query.ToApiModel(), pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<DiscovererModel> GetDiscovererAsync(
            string id, CancellationToken ct) {
            var result = await _client.GetDiscovererAsync(id, 
                ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task UpdateDiscovererAsync(string id, 
            DiscovererUpdateModel request, CancellationToken ct) {
            await _client.UpdateDiscovererAsync(id, request.ToApiModel(), 
                ct).ConfigureAwait(false);
        }

        private readonly IDirectoryServiceApi _client;
    }
}
