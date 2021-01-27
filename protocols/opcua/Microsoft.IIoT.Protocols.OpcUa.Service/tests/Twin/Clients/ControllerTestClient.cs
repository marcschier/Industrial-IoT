// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers.Test {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api;
    using Microsoft.IIoT.Protocols.OpcUa.Api;
    using Microsoft.IIoT.Extensions.Http;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Implementation of twin service api with extra controller methods.
    /// </summary>
    public sealed class ControllerTestClient : ITwinServiceApi {

        /// <summary>
        /// Create test client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="config"></param>
        public ControllerTestClient(IHttpClient httpClient, IOptions<OpcUaApiOptions> options,
            ISerializer serializer) {
            _serviceUri = options.Value.OpcUaServiceUrl?.TrimEnd('/') ??
                throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ??
                throw new ArgumentNullException(nameof(httpClient));
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task<BrowseResponseApiModel> NodeBrowseFirstAsync(string twinId,
            BrowseRequestApiModel content, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var path = new UriBuilder($"{_serviceUri}/v3/nodes/{twinId}/browse");
            if (!string.IsNullOrEmpty(content.NodeId)) {
                path.Query = $"nodeId={content.NodeId.UrlEncode()}";
            }
            var request = _httpClient.NewRequest(path.ToString());
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
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
            var path = new UriBuilder($"{_serviceUri}/v3/nodes/{twinId}/browse/next") {
                Query = $"continuationToken={content.ContinuationToken}"
            };
            var request = _httpClient.NewRequest(path.ToString());
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<BrowseNextResponseApiModel>(response);
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
            if (string.IsNullOrEmpty(content.NodeId)) {
                throw new ArgumentException("Missing nodeid", nameof(content));
            }
            var path = new UriBuilder($"{_serviceUri}/v3/nodes/{twinId}") {
                Query = $"nodeId={content.NodeId.UrlEncode()}"
            };
            var request = _httpClient.NewRequest(path.ToString());
            _serializer.SetAcceptHeaders(request);
            var response = await _httpClient.GetAsync(request, ct).ConfigureAwait(false);
            response.Validate();
            return _serializer.DeserializeResponse<ValueReadResponseApiModel>(response);
        }

        /// <inheritdoc/>
        public Task<ReadResponseApiModel> NodeReadAsync(string twinId,
            ReadRequestApiModel content, CancellationToken ct) {
            return Task.FromException<ReadResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<WriteResponseApiModel> NodeWriteAsync(string twinId,
            WriteRequestApiModel content, CancellationToken ct) {
            return Task.FromException<WriteResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<ValueWriteResponseApiModel> NodeValueWriteAsync(string twinId,
            ValueWriteRequestApiModel content, CancellationToken ct) {
            return Task.FromException<ValueWriteResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<MethodMetadataResponseApiModel> NodeMethodGetMetadataAsync(
            string twinId, MethodMetadataRequestApiModel content, CancellationToken ct) {
            return Task.FromException<MethodMetadataResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<MethodCallResponseApiModel> NodeMethodCallAsync(
            string twinId, MethodCallRequestApiModel content, CancellationToken ct) {
            return Task.FromException<MethodCallResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<BrowsePathResponseApiModel> NodeBrowsePathAsync(string twinId,
            BrowsePathRequestApiModel content, CancellationToken ct) {
            return Task.FromException<BrowsePathResponseApiModel>(new NotImplementedException());
        }

        public Task<ModelUploadStartResponseApiModel> ModelUploadStartAsync(string twinId,
            ModelUploadStartRequestApiModel content, CancellationToken ct) {
            return Task.FromException<ModelUploadStartResponseApiModel>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public Task<string> GetServiceStatusAsync(CancellationToken ct) {
            return Task.FromException<string>(new NotImplementedException());
        }

        public Task<TwinActivationResponseApiModel> ActivateTwinAsync(
            TwinActivationRequestApiModel request, CancellationToken ct) {
            return Task.FromException<TwinActivationResponseApiModel>(new NotImplementedException());
        }

        public Task<TwinInfoListApiModel> ListTwinsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            return Task.FromException<TwinInfoListApiModel>(new NotImplementedException());
        }

        public Task<TwinInfoListApiModel> QueryTwinsAsync(TwinInfoQueryApiModel query,
            int? pageSize, CancellationToken ct) {
            return Task.FromException<TwinInfoListApiModel>(new NotImplementedException());
        }

        public Task<TwinApiModel> GetTwinAsync(string twinId, CancellationToken ct) {
            return Task.FromException<TwinApiModel>(new NotImplementedException());
        }

        public Task UpdateTwinAsync(string twinId, TwinInfoUpdateApiModel model,
            CancellationToken ct) {
            return Task.FromException(new NotImplementedException());
        }

        public Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadRawAsync(
            string twinId, HistoryReadRequestApiModel<VariantValue> request, CancellationToken ct) {
            return Task.FromException<HistoryReadResponseApiModel<VariantValue>>(new NotImplementedException());
        }

        public Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadRawNextAsync(
            string twinId, HistoryReadNextRequestApiModel request, CancellationToken ct) {
            return Task.FromException<HistoryReadNextResponseApiModel<VariantValue>>(new NotImplementedException());
        }

        public Task<HistoryUpdateResponseApiModel> HistoryUpdateRawAsync(
            string twinId, HistoryUpdateRequestApiModel<VariantValue> request, CancellationToken ct) {
            return Task.FromException<HistoryUpdateResponseApiModel>(new NotImplementedException());
        }

        public Task DectivateTwinAsync(string twinId, string generationId, CancellationToken ct) {
            return Task.FromException(new NotImplementedException());
        }

        private readonly IHttpClient _httpClient;
        private readonly ISerializer _serializer;
        private readonly string _serviceUri;
    }
}
