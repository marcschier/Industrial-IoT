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
            var result = await CallServiceOnSupervisorAsync<BrowseRequestModel,
                BrowseResultModel>("Browse_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<BrowseNextResultModel> NodeBrowseNextAsync(
            EndpointInfoModel registration, BrowseNextRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(request.ContinuationToken)) {
                throw new ArgumentNullException(nameof(request.ContinuationToken));
            }
            var result = await CallServiceOnSupervisorAsync<BrowseNextRequestModel,
                BrowseNextResultModel>("BrowseNext_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<BrowsePathResultModel> NodeBrowsePathAsync(
            EndpointInfoModel registration, BrowsePathRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.BrowsePaths == null || request.BrowsePaths.Count == 0 ||
                request.BrowsePaths.Any(p => p == null || p.Length == 0)) {
                throw new ArgumentNullException(nameof(request.BrowsePaths));
            }
            var result = await CallServiceOnSupervisorAsync<BrowsePathRequestModel,
                BrowsePathResultModel>("BrowsePath_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ValueReadResultModel> NodeValueReadAsync(
            EndpointInfoModel registration, ValueReadRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceOnSupervisorAsync<ValueReadRequestModel,
                ValueReadResultModel>("ValueRead_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ValueWriteResultModel> NodeValueWriteAsync(
            EndpointInfoModel registration, ValueWriteRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Value is null) {
                throw new ArgumentNullException(nameof(request.Value));
            }
            var result = await CallServiceOnSupervisorAsync<ValueWriteRequestModel,
                ValueWriteResultModel>("ValueWrite_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<MethodMetadataResultModel> NodeMethodGetMetadataAsync(
            EndpointInfoModel registration, MethodMetadataRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceOnSupervisorAsync<MethodMetadataRequestModel,
                MethodMetadataResultModel>("MethodMetadata_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<MethodCallResultModel> NodeMethodCallAsync(
            EndpointInfoModel registration, MethodCallRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceOnSupervisorAsync<MethodCallRequestModel,
                MethodCallResultModel>("MethodCall_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<ReadResultModel> NodeReadAsync(
            EndpointInfoModel registration, ReadRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Attributes == null || request.Attributes.Count == 0) {
                throw new ArgumentNullException(nameof(request.Attributes));
            }
            if (request.Attributes.Any(r => string.IsNullOrEmpty(r.NodeId))) {
                throw new ArgumentException(nameof(request.Attributes));
            }
            var result = await CallServiceOnSupervisorAsync<ReadRequestModel,
                ReadResultModel>("NodeRead_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<WriteResultModel> NodeWriteAsync(
            EndpointInfoModel registration, WriteRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.Attributes == null || request.Attributes.Count == 0) {
                throw new ArgumentNullException(nameof(request.Attributes));
            }
            if (request.Attributes.Any(r => string.IsNullOrEmpty(r.NodeId))) {
                throw new ArgumentException(nameof(request.Attributes));
            }
            var result = await CallServiceOnSupervisorAsync<WriteRequestModel,
                WriteResultModel>("NodeWrite_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<HistoryReadResultModel<VariantValue>> HistoryReadAsync(
            EndpointInfoModel registration, HistoryReadRequestModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await CallServiceOnSupervisorAsync<
                HistoryReadRequestModel<VariantValue>, HistoryReadResultModel<VariantValue>>(
                "HistoryRead_V2", registration, request);
            return result;
        }

        /// <inheritdoc/>
        public async Task<HistoryReadNextResultModel<VariantValue>> HistoryReadNextAsync(
            EndpointInfoModel registration, HistoryReadNextRequestModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(request.ContinuationToken)) {
                throw new ArgumentNullException(nameof(request.ContinuationToken));
            }
            var result = await CallServiceOnSupervisorAsync<
                HistoryReadNextRequestModel, HistoryReadNextResultModel<VariantValue>>(
                "HistoryRead_V2", registration, request);
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
                throw new ArgumentNullException(nameof(request.Details));
            }
            var result = await CallServiceOnSupervisorAsync<
                HistoryUpdateRequestModel<VariantValue>, HistoryUpdateResultModel>(
                "HistoryUpdate_V2", registration, request);
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
        private async Task<R> CallServiceOnSupervisorAsync<T, R>(string service,
            EndpointInfoModel registration, T request) {
            if (registration == null) {
                throw new ArgumentNullException(nameof(registration));
            }
            if (registration.Endpoint == null) {
                throw new ArgumentNullException(nameof(registration.Endpoint));
            }
            if (string.IsNullOrEmpty(registration.SupervisorId)) {
                throw new ArgumentNullException(nameof(registration.SupervisorId));
            }
            var sw = Stopwatch.StartNew();
            var result = await _client.CallMethodAsync(registration.SupervisorId, service,
                _serializer.SerializeToString(new {
                    endpoint = registration.Endpoint,
                    request
                }));
            _logger.Debug("Calling supervisor service '{service}' on {supervisorId} " +
                "took {elapsed} ms and returned {result}!", service,
                registration.SupervisorId, sw.ElapsedMilliseconds, result);
            return _serializer.Deserialize<R>(result);
        }

        private readonly IJsonSerializer _serializer;
        private readonly IMethodClient _client;
        private readonly ILogger _logger;
    }
}
