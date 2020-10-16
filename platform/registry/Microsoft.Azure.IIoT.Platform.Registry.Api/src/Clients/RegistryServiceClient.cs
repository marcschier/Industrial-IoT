// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
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
            this(httpClient, config?.OpcUaRegistryServiceUrl, serializer) {
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
        public async Task SetDiscoveryModeAsync(string discovererId,
            DiscoveryMode mode, DiscoveryConfigApiModel config, CancellationToken ct) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/discovery/{discovererId}") {
                Query = $"mode={mode}"
            };
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SerializeToRequest(request, config);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }


        /// <inheritdoc/>
        public async Task RegisterAsync(ServerRegistrationRequestApiModel content,
            CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.DiscoveryUrl == null) {
                throw new ArgumentException("Missing discovery url", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            if (request.GetTimeout() == null) {
                request.SetTimeout(TimeSpan.FromMinutes(3));
            }
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task DiscoverAsync(DiscoveryRequestApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications/discover",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            if (request.GetTimeout() == null) {
                request.SetTimeout(TimeSpan.FromMinutes(3));
            }
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task CancelAsync(DiscoveryCancelApiModel content, CancellationToken ct) {
            if (content?.Id == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/applications/discover/${content.Id}", Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationResponseApiModel> RegisterAsync(
            ApplicationRegistrationRequestApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(content.ApplicationUri)) {
                throw new ArgumentException("Missing application uri", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ApplicationRegistrationResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdateApplicationAsync(string applicationId,
            ApplicationInfoUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications/{applicationId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<ApplicationRegistrationApiModel> GetApplicationAsync(
            string applicationId, CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications/{applicationId}",
                Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ApplicationRegistrationApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListApiModel> QueryApplicationsAsync(
            ApplicationRegistrationQueryApiModel query, int? pageSize, CancellationToken ct) {
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications/query",
                Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ApplicationInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListApiModel> ListApplicationsAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications",
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
            return _serializer.DeserializeResponse<ApplicationInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ApplicationSiteListApiModel> ListSitesAsync(
            string continuation, int? pageSize, CancellationToken ct) {
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/applications/sites",
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
            return _serializer.DeserializeResponse<ApplicationSiteListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UnregisterApplicationAsync(string applicationId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/applications/{applicationId}/{generationId}",
                    Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task PurgeDisabledApplicationsAsync(TimeSpan notSeenFor,
            CancellationToken ct) {
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/applications?notSeenFor={notSeenFor}", Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListApiModel> ListEndpointsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/endpoints");
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
            return _serializer.DeserializeResponse<EndpointInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListApiModel> QueryEndpointsAsync(
            EndpointInfoQueryApiModel query, int? pageSize,
            CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/endpoints/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<EndpointInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoApiModel> GetEndpointAsync(string endpointId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/endpoints/{endpointId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<EndpointInfoApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<X509CertificateChainApiModel> GetEndpointCertificateAsync(
            string endpointId, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/endpoints/{endpointId}/certificate");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<X509CertificateChainApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdateEndpointAsync(string endpointId,
            EndpointInfoUpdateApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/endpoints/{endpointId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        private readonly IHttpClient _httpClient;
        private readonly string _serviceUri;
        private readonly ISerializer _serializer;
    }
}
