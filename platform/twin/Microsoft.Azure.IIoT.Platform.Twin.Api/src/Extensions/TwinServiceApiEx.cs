// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api {
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin service api extensions
    /// </summary>
    public static class TwinServiceApiEx {

        /// <summary>
        /// Browse all references if max references is null and user
        /// wants all. If user has requested maximum to return uses
        /// <see cref="ITwinServiceApi.NodeBrowseFirstAsync"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<BrowseResponseApiModel> NodeBrowseAsync(
            this ITwinServiceApi service, string endpoint, BrowseRequestApiModel request,
            CancellationToken ct = default) {
            if (request.MaxReferencesToReturn != null) {
                return await service.NodeBrowseFirstAsync(endpoint, request, ct);
            }
            while (true) {
                // Limit size of batches to a reasonable default to avoid communication timeouts.
                request.MaxReferencesToReturn = 500;
                var result = await service.NodeBrowseFirstAsync(endpoint, request, ct);
                while (result.ContinuationToken != null) {
                    try {
                        var next = await service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Header = request.Header,
                                ReadVariableValues = request.ReadVariableValues,
                                TargetNodesOnly = request.TargetNodesOnly
                            }, ct);
                        result.References.AddRange(next.References);
                        result.ContinuationToken = next.ContinuationToken;
                    }
                    catch (Exception) {
                        await Try.Async(() => service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestApiModel {
                                ContinuationToken = result.ContinuationToken,
                                Abort = true
                            }));
                        throw;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Get list of published nodes
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PublishedItemApiModel>> NodePublishListAllAsync(
            this ITwinServiceApi service, string endpointId) {
            var nodes = new List<PublishedItemApiModel>();
            var result = await service.NodePublishListAsync(endpointId);
            nodes.AddRange(result.Items);
            while (result.ContinuationToken != null) {
                result = await service.NodePublishListAsync(endpointId,
                    result.ContinuationToken);
                nodes.AddRange(result.Items);
            }
            return nodes;
        }

        /// <summary>
        /// Get list of published nodes
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpointId"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public static Task<PublishedItemListResponseApiModel> NodePublishListAsync(
            this ITwinServiceApi service, string endpointId, string continuation = null) {
            return service.NodePublishListAsync(endpointId, new PublishedItemListRequestApiModel {
                ContinuationToken = continuation
            });
        }
        /// <summary>
        /// Read all historic values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllValuesAsync(
            this IHistoryServiceApi client, string endpointId,
            HistoryReadRequestApiModel<ReadValuesDetailsApiModel> request) {
            var result = await client.HistoryReadValuesAsync(endpointId, request);
            return await HistoryReadAllRemainingValuesAsync(client, endpointId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable());
        }

        /// <summary>
        /// Read entire list of modified values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllModifiedValuesAsync(
            this IHistoryServiceApi client, string endpointId,
            HistoryReadRequestApiModel<ReadModifiedValuesDetailsApiModel> request) {
            var result = await client.HistoryReadModifiedValuesAsync(endpointId, request);
            return await HistoryReadAllRemainingValuesAsync(client, endpointId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable());
        }

        /// <summary>
        /// Read entire historic values at specific datum
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllValuesAtTimesAsync(
            this IHistoryServiceApi client, string endpointId,
            HistoryReadRequestApiModel<ReadValuesAtTimesDetailsApiModel> request) {
            var result = await client.HistoryReadValuesAtTimesAsync(endpointId, request);
            return await HistoryReadAllRemainingValuesAsync(client, endpointId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable());
        }

        /// <summary>
        /// Read entire processed historic values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllProcessedValuesAsync(
            this IHistoryServiceApi client, string endpointId,
            HistoryReadRequestApiModel<ReadProcessedValuesDetailsApiModel> request) {
            var result = await client.HistoryReadProcessedValuesAsync(endpointId, request);
            return await HistoryReadAllRemainingValuesAsync(client, endpointId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable());
        }

        /// <summary>
        /// Read entire event history
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricEventApiModel>> HistoryReadAllEventsAsync(
            this IHistoryServiceApi client, string endpointId,
            HistoryReadRequestApiModel<ReadEventsDetailsApiModel> request) {
            var result = await client.HistoryReadEventsAsync(endpointId, request);
            return await HistoryReadAllRemainingEventsAsync(client, endpointId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable());
        }


        /// <summary>
        /// Read all remaining values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="header"></param>
        /// <param name="continuationToken"></param>
        /// <param name="returning"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllRemainingValuesAsync(
            IHistoryServiceApi client, string endpointId, RequestHeaderApiModel header,
            string continuationToken, IEnumerable<HistoricValueApiModel> returning) {
            while (continuationToken != null) {
                var response = await client.HistoryReadValuesNextAsync(endpointId, new HistoryReadNextRequestApiModel {
                    ContinuationToken = continuationToken,
                    Header = header
                });
                continuationToken = response.ContinuationToken;
                returning = returning.Concat(response.History);
            }
            return returning;
        }

        /// <summary>
        /// Read all remaining events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endpointId"></param>
        /// <param name="header"></param>
        /// <param name="continuationToken"></param>
        /// <param name="returning"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<HistoricEventApiModel>> HistoryReadAllRemainingEventsAsync(
            IHistoryServiceApi client, string endpointId, RequestHeaderApiModel header,
            string continuationToken, IEnumerable<HistoricEventApiModel> returning) {
            while (continuationToken != null) {
                var response = await client.HistoryReadEventsNextAsync(endpointId, new HistoryReadNextRequestApiModel {
                    ContinuationToken = continuationToken,
                    Header = header
                });
                continuationToken = response.ContinuationToken;
                returning = returning.Concat(response.History);
            }
            return returning;
        }

    }
}
