// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Api.Clients {
    using Microsoft.IIoT.Platform.Registry.Api.Models;
    using Microsoft.IIoT.Extensions.Http;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implementation of registry service api.
    /// </summary>
    public sealed class RegistryServiceClient : IRegistryServiceApi {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        public RegistryServiceClient(IHttpClient httpClient, IRegistryConfig config,
            ISerializer serializer) :
            this(httpClient, config?.RegistryServiceUrl, serializer) {
        }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="serviceUri"></param>
        /// <param name="serializer"></param>
        public RegistryServiceClient(IHttpClient httpClient, string serviceUri,
            ISerializer serializer = null) {
            if (string.IsNullOrWhiteSpace(serviceUri)) {
                throw new ArgumentNullException(nameof(serviceUri),
                    "Please configure the Url of the registry micro service.");
            }
            _serviceUri = serviceUri.TrimEnd('/');
            _serializer = serializer ?? new NewtonSoftJsonSerializer();
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc/>
        public async Task<string> GetServiceStatusAsync(CancellationToken ct) {
            var request = _httpClient.NewRequest($"{_serviceUri}/healthz",
                Resource.Platform);
            try {
                var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
                response.Validate();
                return response.GetContentAsString();
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateDiscovererAsync(string discovererId,
            DiscovererUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/discovery/{discovererId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<DiscovererListApiModel> ListDiscoverersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/discovery");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (continuation != null) {
                request.AddHeader(HttpHeader.ContinuationToken, continuation);
            }
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<DiscovererListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<DiscovererListApiModel> QueryDiscoverersAsync(
            DiscovererQueryApiModel query, int? pageSize,
            CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/discovery/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<DiscovererListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<DiscovererApiModel> GetDiscovererAsync(
            string discovererId, CancellationToken ct) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/discovery/{discovererId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<DiscovererApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdateSupervisorAsync(string supervisorId,
            SupervisorUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(supervisorId)) {
                throw new ArgumentNullException(nameof(supervisorId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/supervisors/{supervisorId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<SupervisorListApiModel> ListSupervisorsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/supervisors");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (continuation != null) {
                request.AddHeader(HttpHeader.ContinuationToken, continuation);
            }
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<SupervisorListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<SupervisorListApiModel> QuerySupervisorsAsync(
            SupervisorQueryApiModel query, int? pageSize,
            CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/supervisors/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<SupervisorListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<SupervisorApiModel> GetSupervisorAsync(
            string supervisorId, CancellationToken ct) {
            if (string.IsNullOrEmpty(supervisorId)) {
                throw new ArgumentNullException(nameof(supervisorId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/supervisors/{supervisorId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<SupervisorApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<PublisherListApiModel> ListPublishersAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/publishers");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (continuation != null) {
                request.AddHeader(HttpHeader.ContinuationToken, continuation);
            }
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<PublisherListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdatePublisherAsync(string publisherId,
            PublisherUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(publisherId)) {
                throw new ArgumentNullException(nameof(publisherId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/publishers/{publisherId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<PublisherListApiModel> QueryPublishersAsync(
            PublisherQueryApiModel query, int? pageSize,
            CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/publishers/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<PublisherListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<PublisherApiModel> GetPublisherAsync(
            string publisherId, CancellationToken ct) {
            if (string.IsNullOrEmpty(publisherId)) {
                throw new ArgumentNullException(nameof(publisherId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/publishers/{publisherId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<PublisherApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<GatewayListApiModel> ListGatewaysAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/gateways");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (continuation != null) {
                request.AddHeader(HttpHeader.ContinuationToken, continuation);
            }
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<GatewayListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdateGatewayAsync(string gatewayId,
            GatewayUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(gatewayId)) {
                throw new ArgumentNullException(nameof(gatewayId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/gateways/{gatewayId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<GatewayListApiModel> QueryGatewaysAsync(
            GatewayQueryApiModel query, int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/gateways/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<GatewayListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<GatewayInfoApiModel> GetGatewayAsync(string gatewayId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(gatewayId)) {
                throw new ArgumentNullException(nameof(gatewayId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/gateways/{gatewayId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<GatewayInfoApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<GatewaySiteListApiModel> ListSitesAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/gateways/sites",
                Resource.Platform);
            if (continuation != null) {
                request.AddHeader(HttpHeader.ContinuationToken, continuation);
            }
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<GatewaySiteListApiModel>(response);
        }

        private readonly IHttpClient _httpClient;
        private readonly string _serviceUri;
        private readonly ISerializer _serializer;
    }
}
