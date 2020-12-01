// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api {
    using Microsoft.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.IIoT.Platform.Events.Api;
    using Microsoft.IIoT.Rpc;
    using Microsoft.IIoT.Http;
    using Microsoft.IIoT.Serializers.NewtonSoft;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Utils;
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
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/groups/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.GroupEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetWriterEventsAsync(
            Func<DataSetWriterEventApiModel, Task> callback) {
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/writers/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.WriterEventTarget, callback);
            return new AsyncDisposable(registration);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetItemStatusAsync(
            string dataSetWriterId, Func<PublishedDataSetItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/writers/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DataSetItemEventTarget, callback);
            try {
                await SubscribeDataSetItemStatusAsync(dataSetWriterId, hub.ConnectionId,
                    CancellationToken.None).ConfigureAwait(false);
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
        public async Task<IAsyncDisposable> SubscribeEventDataSetMessagesAsync(
            string dataSetWriterId, Func<PublishedDataSetItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/writers/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DataSetEventMessageTarget, callback);
            try {
                await SubscribeDataSetEventMessagesAsync(dataSetWriterId, hub.ConnectionId,
                    CancellationToken.None).ConfigureAwait(false);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDataSetEventMessagesAsync(dataSetWriterId,
                        hub.ConnectionId, CancellationToken.None));
            }
            catch {
                registration.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeDataSetVariableMessagesAsync(
            string dataSetWriterId, string variableId,
            Func<PublishedDataSetItemMessageApiModel, Task> callback) {

            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            var hub = await _client.GetHubAsync($"{_serviceUri}/v3/writers/events",
                Resource.Platform).ConfigureAwait(false);
            var registration = hub.Register(EventTargets.DataSetVariableMessageTarget, callback);
            try {
                await SubscribeDataSetVariableMessagesAsync(dataSetWriterId, variableId,
                    hub.ConnectionId, CancellationToken.None).ConfigureAwait(false);
                return new AsyncDisposable(registration,
                    () => UnsubscribeDataSetVariableMessagesAsync(dataSetWriterId, variableId,
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
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/status", Resource.Platform);
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
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/status/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task SubscribeDataSetVariableMessagesAsync(string dataSetWriterId,
            string variableId, string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/variables/{variableId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDataSetVariableMessagesAsync(string dataSetWriterId,
            string variableId, string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/variables/{variableId}/{connectionId}",
                Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task SubscribeDataSetEventMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/event", Resource.Platform);
            _serializer.SerializeToRequest(request, connectionId);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeDataSetEventMessagesAsync(string dataSetWriterId,
            string connectionId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(connectionId)) {
                throw new ArgumentNullException(nameof(connectionId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/writers/{dataSetWriterId}/event/{connectionId}",
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
