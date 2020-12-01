// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api {
    using Microsoft.IIoT.Platform.Discovery.Api.Models;
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Rpc;
    using Microsoft.IIoT.Http;
    using Microsoft.IIoT.Utils;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Discovery service event client
    /// </summary>
    public class DiscoveryServiceEvents : IDiscoveryServiceEvents, IDiscoveryEventApi {

        /// <summary>
        /// Event client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="client"></param>
        public DiscoveryServiceEvents(IHttpClient httpClient, ICallbackClient client,
            IEventsConfig config, ISerializer serializer) :
            this(httpClient, client, config?.OpcUaEventsServiceUrl, serializer) {
        }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="client"></param>
        /// <param name="serviceUri"></param>
        /// <param name="serializer"></param>
        public DiscoveryServiceEvents(IHttpClient httpClient, ICallbackClient client,
            string serviceUri, ISerializer serializer = null) {
            if (string.IsNullOrWhiteSpace(serviceUri)) {
                throw new ArgumentNullException(nameof(serviceUri),
                    "Please configure the Url of the events micro service.");
            }
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serviceUri = serviceUri.TrimEnd('/');
            _serializer = serializer ?? new NewtonSoftJsonSerializer();
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeApplicationEventsAsync(
            Func<ApplicationEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/applications/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.ApplicationEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeEndpointEventsAsync(
            Func<EndpointEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/endpoints/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.EndpointEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDiscoveryProgressByDiscovererIdAsync(
            string discovererId, Func<DiscoveryProgressApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/discovery/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DiscoveryProgressTarget, callback);
            try {
                await SubscribeDiscoveryProgressByDiscovererIdAsync(discovererId,
                    hub.ConnectionId, CancellationToken.None).ConfigureAwait(false);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDiscoveryProgressByDiscovererIdAsync(discovererId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDiscoveryProgressByRequestIdAsync(
            string requestId, Func<DiscoveryProgressApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/discovery/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DiscoveryProgressTarget, callback);
            try {
                await SubscribeDiscoveryProgressByRequestIdAsync(requestId, hub.ConnectionId,
                    CancellationToken.None).ConfigureAwait(false);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDiscoveryProgressByRequestIdAsync(requestId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeDiscoveryProgressByDiscovererIdAsync(string discovererId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/discovery/{discovererId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task SubscribeDiscoveryProgressByRequestIdAsync(string requestId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/discovery/requests/{requestId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDiscoveryProgressByDiscovererIdAsync(string discovererId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(discovererId)) {
                throw new ArgumentNullException(nameof(discovererId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/discovery/{discovererId}/events/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDiscoveryProgressByRequestIdAsync(string requestId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/discovery/requests/{requestId}/events/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        private readonly IHttpClient _httpClient;
        private readonly string _serviceUri;
        private readonly ISerializer _serializer;
        private readonly ICallbackClient _client;
    }
}
