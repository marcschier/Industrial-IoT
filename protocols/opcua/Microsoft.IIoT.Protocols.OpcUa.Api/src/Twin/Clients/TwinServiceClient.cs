// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Clients {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Extensions.Http;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Extensions.Serializers.NewtonSoft;
    using System;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Implementation of twinId service api.
    /// </summary>
    public sealed class TwinServiceClient : ITwinServiceApi, IHistoryServiceApi {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        public TwinServiceClient(IHttpClient httpClient, ITwinConfig config,
            ISerializer serializer) :
            this(httpClient, config?.OpcUaTwinServiceUrl, serializer) {
        }

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="serviceUri"></param>
        /// <param name="serializer"></param>
        public TwinServiceClient(IHttpClient httpClient, string serviceUri,
            ISerializer serializer = null) {
            if (string.IsNullOrWhiteSpace(serviceUri)) {
                throw new ArgumentNullException(nameof(serviceUri),
                    "Please configure the Url of the twinId micro service.");
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
        public async Task<TwinActivationResponseApiModel> ActivateTwinAsync(
            TwinActivationRequestApiModel content, CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(content.EndpointId)) {
                throw new ArgumentException("Missing endpoint id", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/twins", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PutAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<TwinActivationResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task UpdateTwinAsync(string twinId, TwinInfoUpdateApiModel content,
            CancellationToken ct) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/twins/{twinId}", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PatchAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task DectivateTwinAsync(string twinId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/twins/{twinId}/{generationId}", Resource.Platform);
            var response = await _httpClient.DeleteAsync(request, ct).ConfigureAwait(false);
            response.Validate();
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListApiModel> ListTwinsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/twins");
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
            return _serializer.DeserializeResponse<TwinInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListApiModel> QueryTwinsAsync(
            TwinInfoQueryApiModel query, int? pageSize,
            CancellationToken ct) {
            var uri = new UriBuilder($"{_serviceUri}/v3/twins/query");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            if (pageSize != null) {
                request.AddHeader(HttpHeader.MaxItemCount, pageSize.ToString());
            }
            _serializer.SerializeToRequest(request, query);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<TwinInfoListApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<TwinApiModel> GetTwinAsync(string twinId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var uri = new UriBuilder($"{_serviceUri}/v3/twins/{twinId}");
            var request = _httpClient.NewRequest(uri.Uri, Resource.Platform);
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<TwinApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<BrowseResponseApiModel> NodeBrowseFirstAsync(string twinId,
            BrowseRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/browse/{twinId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<BrowseResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResponseApiModel> NodeBrowseNextAsync(string twinId,
            BrowseNextRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.ContinuationToken == null) {
                throw new ArgumentException("Missing continuation", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/browse/{twinId}/next",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<BrowseNextResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResponseApiModel> NodeBrowsePathAsync(string twinId,
            BrowsePathRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.BrowsePaths == null || content.BrowsePaths.Count == 0 ||
                content.BrowsePaths.Any(p => p == null || p.Count == 0)) {
                throw new ArgumentException("Bad browse paths", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/browse/{twinId}/path",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<BrowsePathResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ReadResponseApiModel> NodeReadAsync(string twinId,
            ReadRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Attributes == null || content.Attributes.Count == 0) {
                throw new ArgumentException(nameof(content.Attributes));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/attributes", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ReadResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<WriteResponseApiModel> NodeWriteAsync(string twinId,
            WriteRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Attributes == null || content.Attributes.Count == 0) {
                throw new ArgumentException(nameof(content.Attributes));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/write/{twinId}/attributes", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<WriteResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ValueReadResponseApiModel> NodeValueReadAsync(string twinId,
            ValueReadRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/read/{twinId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ValueReadResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResponseApiModel> NodeValueWriteAsync(string twinId,
            ValueWriteRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Value == null) {
                throw new ArgumentException("Missing value", nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/write/{twinId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ValueWriteResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResponseApiModel> NodeMethodGetMetadataAsync(
            string twinId, MethodMetadataRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/call/{twinId}/metadata",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<MethodMetadataResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<MethodCallResponseApiModel> NodeMethodCallAsync(
            string twinId, MethodCallRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/call/{twinId}",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<MethodCallResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<ModelUploadStartResponseApiModel> ModelUploadStartAsync(
            string twinId, ModelUploadStartRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest($"{_serviceUri}/v3/transfer/{twinId}/model",
                Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ModelUploadStartResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadRawAsync(
            string twinId, HistoryReadRequestApiModel<VariantValue> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Details == null) {
                throw new ArgumentException("Missing details", nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/history/read/{twinId}", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<VariantValue>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadRawNextAsync(
            string twinId, HistoryReadNextRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(content.ContinuationToken)) {
                throw new ArgumentException("Missing continuation", nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/history/read/{twinId}/next", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadNextResponseApiModel<VariantValue>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryUpdateRawAsync(
            string twinId, HistoryUpdateRequestApiModel<VariantValue> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (content.Details == null) {
                throw new ArgumentException("Missing details", nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/history/update/{twinId}", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadValuesAsync(
            string twinId, HistoryReadRequestApiModel<ReadValuesDetailsApiModel> content,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/values", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<HistoricValueApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadModifiedValuesAsync(
            string twinId, HistoryReadRequestApiModel<ReadModifiedValuesDetailsApiModel> content,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/values/modified", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<HistoricValueApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadValuesAtTimesAsync(
            string twinId, HistoryReadRequestApiModel<ReadValuesAtTimesDetailsApiModel> content,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/values/pick", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<HistoricValueApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadProcessedValuesAsync(
            string twinId, HistoryReadRequestApiModel<ReadProcessedValuesDetailsApiModel> content,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/values/processed", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<HistoricValueApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResponseApiModel<HistoricValueApiModel[]>> HistoryReadValuesNextAsync(
            string twinId, HistoryReadNextRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(content.ContinuationToken)) {
                throw new ArgumentException("Missing continuation", nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/values/next", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadNextResponseApiModel<HistoricValueApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResponseApiModel<HistoricEventApiModel[]>> HistoryReadEventsAsync(
            string twinId, HistoryReadRequestApiModel<ReadEventsDetailsApiModel> content,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadResponseApiModel<HistoricEventApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResponseApiModel<HistoricEventApiModel[]>> HistoryReadEventsNextAsync(
            string twinId, HistoryReadNextRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(content.ContinuationToken)) {
                throw new ArgumentException("Missing continuation", nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/read/{twinId}/events/next", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryReadNextResponseApiModel<HistoricEventApiModel[]>>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryReplaceValuesAsync(string twinId,
            HistoryUpdateRequestApiModel<ReplaceValuesDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/replace/{twinId}/values", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryReplaceEventsAsync(string twinId,
            HistoryUpdateRequestApiModel<ReplaceEventsDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/replace/{twinId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertValuesAsync(string twinId,
            HistoryUpdateRequestApiModel<InsertValuesDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/insert/{twinId}/values", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertEventsAsync(string twinId,
            HistoryUpdateRequestApiModel<InsertEventsDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/insert/{twinId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteValuesAsync(string twinId,
            HistoryUpdateRequestApiModel<DeleteValuesDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/delete/{twinId}/values", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteValuesAtTimesAsync(string twinId,
            HistoryUpdateRequestApiModel<DeleteValuesAtTimesDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/delete/{twinId}/values/pick", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteModifiedValuesAsync(string twinId,
            HistoryUpdateRequestApiModel<DeleteModifiedValuesDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/delete/{twinId}/values/modified", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteEventsAsync(string twinId,
            HistoryUpdateRequestApiModel<DeleteEventsDetailsApiModel> content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            var request = _httpClient.NewRequest(
                $"{_serviceUri}/v3/delete/{twinId}/events", Resource.Platform);
            _serializer.SerializeToRequest(request, content);
            var response = await _httpClient.PostAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<HistoryUpdateResponseApiModel>(response);
        }

        private readonly IHttpClient _httpClient;
        private readonly ISerializer _serializer;
        private readonly string _serviceUri;
    }
}
