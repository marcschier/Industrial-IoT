// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Events.Api;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Http;
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Publisher service events
    /// </summary>
    public class PublisherServiceEvents : IPublisherServiceEvents, IPublisherEventApi {

        /// <summary>
        /// Event client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="client"></param>
        public PublisherServiceEvents(IHttpClient httpClient, ICallbackClient client,
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
        public PublisherServiceEvents(IHttpClient httpClient, ICallbackClient client,
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
        public async Task<IAsyncDisposable> SubscribeWriterGroupEventsAsync(
            Func<WriterGroupEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v2/groups/events",
                Resource.Platform);
            var registration = hub.Register(EventTargets.GroupEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetWriterMessagesAsync(
            string dataSetWriterId, Func<MonitoredItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v2/groups/events",
                Resource.Platform);
            var registration = hub.Register(EventTargets.DataSetMessageTarget, callback);
            try {
                await SubscribeDataSetWriterMessagesAsync(dataSetWriterId, hub.ConnectionId,
                    CancellationToken.None);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDataSetWriterMessagesAsync(dataSetWriterId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetWriterEventsAsync(
            Func<DataSetWriterEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v2/writers/events",
                Resource.Platform);
            var registration = hub.Register(EventTargets.WriterEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetItemStatusAsync(
            string dataSetWriterId, Func<MonitoredItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v2/writers/events",
                Resource.Platform);
            var registration = hub.Register(EventTargets.DataSetEventTarget, callback);
            try {
                await SubscribeDataSetItemStatusAsync(dataSetWriterId, hub.ConnectionId,
                    CancellationToken.None);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDataSetItemStatusAsync(dataSetWriterId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> NodePublishSubscribeByEndpointAsync(
            string endpointId, Func<MonitoredItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v2/publishers/events",
                Resource.Platform);
            var registration = hub.Register(EventTargets.PublisherSampleTarget, callback);
            try {
                await NodePublishSubscribeByEndpointAsync(endpointId, hub.ConnectionId,
                    CancellationToken.None);
                return new AsyncDisposable(registration,
                    () => NodePublishUnsubscribeByEndpointAsync(endpointId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/writers/{dataSetWriterId}/status", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDataSetItemStatusAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/writers/{dataSetWriterId}/status/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task SubscribeDataSetWriterMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/groups/{dataSetWriterId}/messages", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDataSetWriterMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/groups/{dataSetWriterId}/messages/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task NodePublishSubscribeByEndpointAsync(string endpointId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/endpoints/{endpointId}/samples", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task NodePublishUnsubscribeByEndpointAsync(string endpointId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v2/endpoints/{endpointId}/samples/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        private readonly IHttpClient _httpClient;
        private readonly ISerializer _serializer;
        private readonly string _serviceUri;
        private readonly ICallbackClient _client;
    }
}
