// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api {
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Api extensions
    /// </summary>
    public static class TwinModuleApiEx {

        /// <summary>
        /// Browse node on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<BrowseResponseApiModel> NodeBrowseAsync(
            this ITwinModuleApi api, string endpointUrl, BrowseRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeBrowseAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Browse node on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<BrowseResponseApiModel> NodeBrowseFirstAsync(
            this ITwinModuleApi api, string endpointUrl, BrowseRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeBrowseFirstAsync(new EndpointApiModel {
                Url = endpointUrl,
                SecurityMode = SecurityMode.None
            }, request, ct);
        }

        /// <summary>
        /// Browse next references on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<BrowseNextResponseApiModel> NodeBrowseNextAsync(
            this ITwinModuleApi api, string endpointUrl, BrowseNextRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeBrowseNextAsync(new EndpointApiModel {
                Url = endpointUrl,
                SecurityMode = SecurityMode.None
            }, request, ct);
        }

        /// <summary>
        /// Browse by path on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<BrowsePathResponseApiModel> NodeBrowsePathAsync(
            this ITwinModuleApi api, string endpointUrl, BrowsePathRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeBrowsePathAsync(new EndpointApiModel {
                Url = endpointUrl,
                SecurityMode = SecurityMode.None
            }, request, ct);
        }

        /// <summary>
        /// Call method on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<MethodCallResponseApiModel> NodeMethodCallAsync(
            this ITwinModuleApi api, string endpointUrl, MethodCallRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeMethodCallAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Get meta data for method call on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<MethodMetadataResponseApiModel> NodeMethodGetMetadataAsync(
            this ITwinModuleApi api, string endpointUrl, MethodMetadataRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeMethodGetMetadataAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Read node value on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ValueReadResponseApiModel> NodeValueReadAsync(
            this ITwinModuleApi api, string endpointUrl, ValueReadRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeValueReadAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Write node value on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ValueWriteResponseApiModel> NodeValueWriteAsync(
            this ITwinModuleApi api, string endpointUrl, ValueWriteRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeValueWriteAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Read node attributes on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<ReadResponseApiModel> NodeReadAsync(
            this ITwinModuleApi api, string endpointUrl, ReadRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeReadAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Write node attributes on endpoint
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<WriteResponseApiModel> NodeWriteAsync(
            this ITwinModuleApi api, string endpointUrl, WriteRequestApiModel request,
            CancellationToken ct = default) {
            return api.NodeWriteAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Browse all references if max references == null and user
        /// wants all. If user has requested maximum to return uses
        /// <see cref="ITwinModuleApi.NodeBrowseFirstAsync"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<BrowseResponseApiModel> NodeBrowseAsync(
            this ITwinModuleApi service, EndpointApiModel endpoint,
            BrowseRequestApiModel request, CancellationToken ct = default) {
            if (request.MaxReferencesToReturn != null) {
                return await service.NodeBrowseFirstAsync(endpoint, request, 
                    ct).ConfigureAwait(false);
            }
            while (true) {
                var result = await service.NodeBrowseFirstAsync(endpoint, request,
                    ct).ConfigureAwait(false);
                while (result.ContinuationToken != null) {
                    try {
                        var next = await service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Header = request.Header,
                                ReadVariableValues = request.ReadVariableValues,
                                TargetNodesOnly = request.TargetNodesOnly
                            }, ct).ConfigureAwait(false);
                        result.References.AddRange(next.References);
                        result.ContinuationToken = next.ContinuationToken;
                    }
                    catch (Exception) {
                        await Try.Async(() => service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Abort = true
                            })).ConfigureAwait(false);
                        throw;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Read node history with custom encoded extension object details
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadRawAsync(
            this IHistoryModuleApi api, string endpointUrl,
            HistoryReadRequestApiModel<VariantValue> request,
            CancellationToken ct = default) {
            return api.HistoryReadRawAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Read history call with custom encoded extension object details
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadRawNextAsync(
            this IHistoryModuleApi api, string endpointUrl,
            HistoryReadNextRequestApiModel request,
            CancellationToken ct = default) {
            return api.HistoryReadRawNextAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// Update using raw extension object details
        /// </summary>
        /// <param name="api"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<HistoryUpdateResponseApiModel> HistoryUpdateRawAsync(
            this IHistoryModuleApi api, string endpointUrl, 
            HistoryUpdateRequestApiModel<VariantValue> request,
            CancellationToken ct = default) {
            return api.HistoryUpdateRawAsync(NewEndpoint(endpointUrl), request, ct);
        }

        /// <summary>
        /// New endpoint
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        private static EndpointApiModel NewEndpoint(string endpointUrl) {
            return new EndpointApiModel {
                Url = endpointUrl,
                SecurityMode = SecurityMode.None,
                SecurityPolicy = "None"
            };
        }
    }
}
