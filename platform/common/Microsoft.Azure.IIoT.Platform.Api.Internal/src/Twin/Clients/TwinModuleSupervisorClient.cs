// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Clients {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents the supervisor api surface.
    /// </summary>
    public sealed class TwinModuleSupervisorClient : IBrowseServices<EndpointInfoModel>,
        IHistoricAccessServices<EndpointInfoModel>,
        INodeServices<EndpointInfoModel> {

        /// <summary>
        /// Create service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public TwinModuleSupervisorClient(IMethodClient client, IJsonSerializer serializer,
            ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<BrowseResultModel> NodeBrowseFirstAsync(
            EndpointInfoModel registration, BrowseRequestModel request) {
            var result = await CallServiceAsync<BrowseRequestModel,
                BrowseResultModel>("Browse_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            EndpointInfoModel registration, BrowseNextRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(request.ContinuationToken)) {
                throw new ArgumentException("Missing continuation", nameof(request));
            }
            var result = await CallServiceAsync<BrowseNextRequestModel,
                BrowseNextResultModel>("BrowseNext_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            EndpointInfoModel registration, BrowsePathRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.BrowsePaths == null || request.BrowsePaths.Count == 0 ||
                request.BrowsePaths.Any(p => p == null || p.Count == 0)) {
                throw new ArgumentException("Bad browse paths", nameof(request));
            }
            var result = await CallServiceAsync<BrowsePathRequestModel,
                BrowsePathResultModel>("BrowsePath_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            EndpointInfoModel registration, ValueReadRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceAsync<ValueReadRequestModel,
                ValueReadResultModel>("ValueRead_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            EndpointInfoModel registration, ValueWriteRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Value is null) {
                throw new ArgumentException("Missing value", nameof(request));
            }
            var result = await CallServiceAsync<ValueWriteRequestModel,
                ValueWriteResultModel>("ValueWrite_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            EndpointInfoModel registration, MethodMetadataRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceAsync<MethodMetadataRequestModel,
                MethodMetadataResultModel>("MethodMetadata_V2", registration, 
                request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            EndpointInfoModel registration, MethodCallRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceAsync<MethodCallRequestModel,
                MethodCallResultModel>("MethodCall_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            EndpointInfoModel registration, ReadRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Attributes == null || request.Attributes.Count == 0) {
                throw new ArgumentException("Missing attributes", nameof(request));
            }
            if (request.Attributes.Any(r => string.IsNullOrEmpty(r.NodeId))) {
                throw new ArgumentException("Missing attribute node ids", nameof(request));
            }
            var result = await CallServiceAsync<ReadRequestModel,
                ReadResultModel>("NodeRead_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            EndpointInfoModel registration, WriteRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Attributes == null || request.Attributes.Count == 0) {
                throw new ArgumentException("Missing attributes", nameof(request));
            }
            if (request.Attributes.Any(r => string.IsNullOrEmpty(r.NodeId))) {
                throw new ArgumentException("Missing attribute node ids", nameof(request));
            }
            var result = await CallServiceAsync<WriteRequestModel,
                WriteResultModel>("NodeWrite_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            EndpointInfoModel registration, HistoryReadRequestModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceAsync<
                HistoryReadRequestModel<VariantValue>, HistoryReadResultModel<VariantValue>>(
                "HistoryRead_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            EndpointInfoModel registration, HistoryReadNextRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(request.ContinuationToken)) {
                throw new ArgumentException("Missing continuation", nameof(request));
            }
            var result = await CallServiceAsync<
                HistoryReadNextRequestModel, HistoryReadNextResultModel<VariantValue>>(
                "HistoryRead_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<HistoryUpdateResultModel> HistoryUpdateAsync(
            EndpointInfoModel registration,
            HistoryUpdateRequestModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Details == null) {
                throw new ArgumentException("Missing details", nameof(request));
            }
            var result = await CallServiceAsync<
                HistoryUpdateRequestModel<VariantValue>, HistoryUpdateResultModel>(
                "HistoryUpdate_V2", registration, request).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// helper to invoke service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="service"></param>
        /// <param name="registration"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<R> CallServiceAsync<T, R>(string service,
            EndpointInfoModel registration, T request) {
            if (registration == null) {
                throw new ArgumentNullException(nameof(registration));
            }
            if (registration.Endpoint == null) {
                throw new ArgumentException("Missing endpoint", nameof(registration));
            }
            var sw = Stopwatch.StartNew();

            // TODO
            string target = null; // registration.SupervisorId;

            var result = await _client.CallMethodAsync(target, service,
                _serializer.SerializeToString(new {
                    endpoint = registration.Endpoint,
                    request
                })).ConfigureAwait(false);
            _logger.Debug("Calling service '{service}' on {target} " +
                "took {elapsed} ms and returned {result}!", service,
                target, sw.ElapsedMilliseconds, result);
            return _serializer.Deserialize<R>(result);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
