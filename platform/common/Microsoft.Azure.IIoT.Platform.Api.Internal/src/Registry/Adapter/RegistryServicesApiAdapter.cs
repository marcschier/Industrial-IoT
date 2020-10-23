// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Registry services adapter to run dependent services outside of cloud.
    /// </summary>
    public sealed class RegistryServicesApiAdapter : IEndpointRegistry,
        IApplicationRegistry, IDiscoveryServices {

        /// <summary>
        /// Create registry services
        /// </summary>
        /// <param name="client"></param>
        public RegistryServicesApiAdapter(IRegistryServiceApi client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> GetEndpointAsync(string id,
            CancellationToken ct) {
            var result = await _client.GetEndpointAsync(id, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> ListEndpointsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListEndpointsAsync(continuation,
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> QueryEndpointsAsync(
            EndpointInfoQueryModel query, int? pageSize, CancellationToken ct) {
            var result = await _client.QueryEndpointsAsync(query.ToApiModel(),
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationResultModel> RegisterApplicationAsync(
            ApplicationRegistrationRequestModel request, OperationContextModel context, 
            CancellationToken ct) {
            var result = await _client.RegisterAsync(request.ToApiModel(), 
                ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationModel> GetApplicationAsync(
            string applicationId, CancellationToken ct) {
            var result = await _client.GetApplicationAsync(applicationId, 
                ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateModel request, OperationContextModel context,
            CancellationToken ct) {
            return _client.UpdateApplicationAsync(applicationId, request.ToApiModel(), ct);
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListModel> ListApplicationsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var result = await _client.ListApplicationsAsync(continuation, 
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListModel> QueryApplicationsAsync(
            ApplicationInfoQueryModel query, int? pageSize, CancellationToken ct) {
            var result = await _client.QueryApplicationsAsync(query.ToApiModel(),
                pageSize, ct).ConfigureAwait(false);
            return result.ToServiceModel();
        }

        /// <inheritdoc/>
        public Task UnregisterApplicationAsync(string applicationId, string generationId,
            OperationContextModel context, CancellationToken ct) {
            return _client.UnregisterApplicationAsync(applicationId, generationId, ct);
        }

        /// <inheritdoc/>
        public Task PurgeLostApplicationsAsync(TimeSpan notSeenFor,
            OperationContextModel context, CancellationToken ct) {
            return _client.PurgeDisabledApplicationsAsync(notSeenFor, ct);
        }

        /// <inheritdoc/>
        public Task DiscoverAsync(DiscoveryRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            return _client.DiscoverAsync(request.ToApiModel(), ct);
        }

        /// <inheritdoc/>
        public Task CancelAsync(DiscoveryCancelModel request, 
            OperationContextModel context, CancellationToken ct) {
            return _client.CancelAsync(request.ToApiModel(), ct);
        }

        /// <inheritdoc/>
        public Task RegisterAsync(ServerRegistrationRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            return _client.RegisterAsync(request.ToApiModel(), ct);
        }

        private readonly IRegistryServiceApi _client;
    }
}
