// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using Microsoft.IIoT.Extensions.Utils;
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
        /// <param name="twin"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<BrowseResponseApiModel> NodeBrowseAsync(
            this ITwinServiceApi service, string twin, BrowseRequestApiModel request,
            CancellationToken ct = default) {
            if (request.MaxReferencesToReturn != null) {
                return await service.NodeBrowseFirstAsync(twin, request,
                    ct).ConfigureAwait(false);
            }
            while (true) {
                // Limit size of batches to a reasonable default to avoid communication timeouts.
                request.MaxReferencesToReturn = 500;
                var result = await service.NodeBrowseFirstAsync(twin, request,
                    ct).ConfigureAwait(false);
                while (result.ContinuationToken != null) {
                    try {
                        var next = await service.NodeBrowseNextAsync(twin,
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
                        await Try.Async(() => service.NodeBrowseNextAsync(twin,
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
        /// Read all historic values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllValuesAsync(
            this IHistoryServiceApi client, string twinId,
            HistoryReadRequestApiModel<ReadValuesDetailsApiModel> request,
            CancellationToken ct = default) {
            var result = await client.HistoryReadValuesAsync(twinId, request,
                ct).ConfigureAwait(false);
            return await HistoryReadAllRemainingValuesAsync(client, twinId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Read entire list of modified values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllModifiedValuesAsync(
            this IHistoryServiceApi client, string twinId,
            HistoryReadRequestApiModel<ReadModifiedValuesDetailsApiModel> request,
            CancellationToken ct = default) {
            var result = await client.HistoryReadModifiedValuesAsync(twinId, request,
                ct).ConfigureAwait(false);
            return await HistoryReadAllRemainingValuesAsync(client, twinId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Read entire historic values at specific datum
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllValuesAtTimesAsync(
            this IHistoryServiceApi client, string twinId,
            HistoryReadRequestApiModel<ReadValuesAtTimesDetailsApiModel> request,
            CancellationToken ct = default) {
            var result = await client.HistoryReadValuesAtTimesAsync(twinId, request,
                ct).ConfigureAwait(false);
            return await HistoryReadAllRemainingValuesAsync(client, twinId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Read entire processed historic values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllProcessedValuesAsync(
            this IHistoryServiceApi client, string twinId,
            HistoryReadRequestApiModel<ReadProcessedValuesDetailsApiModel> request,
            CancellationToken ct = default) {
            var result = await client.HistoryReadProcessedValuesAsync(twinId, request,
                ct).ConfigureAwait(false);
            return await HistoryReadAllRemainingValuesAsync(client, twinId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Read entire event history
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<HistoricEventApiModel>> HistoryReadAllEventsAsync(
            this IHistoryServiceApi client, string twinId,
            HistoryReadRequestApiModel<ReadEventsDetailsApiModel> request,
            CancellationToken ct = default) {
            var result = await client.HistoryReadEventsAsync(twinId, request,
                ct).ConfigureAwait(false);
            return await HistoryReadAllRemainingEventsAsync(client, twinId, request.Header,
                result.ContinuationToken, result.History.AsEnumerable(), ct).ConfigureAwait(false);
        }


        /// <summary>
        /// Read all remaining values
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="header"></param>
        /// <param name="continuationToken"></param>
        /// <param name="returning"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<HistoricValueApiModel>> HistoryReadAllRemainingValuesAsync(
            IHistoryServiceApi client, string twinId, RequestHeaderApiModel header,
            string continuationToken, IEnumerable<HistoricValueApiModel> returning,
            CancellationToken ct = default) {
            while (continuationToken != null) {
                var response = await client.HistoryReadValuesNextAsync(twinId,
                    new HistoryReadNextRequestApiModel {
                        ContinuationToken = continuationToken,
                        Header = header
                    }, ct).ConfigureAwait(false);
                continuationToken = response.ContinuationToken;
                returning = returning.Concat(response.History);
            }
            return returning;
        }

        /// <summary>
        /// Read all remaining events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="twinId"></param>
        /// <param name="header"></param>
        /// <param name="continuationToken"></param>
        /// <param name="returning"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<HistoricEventApiModel>> HistoryReadAllRemainingEventsAsync(
            IHistoryServiceApi client, string twinId, RequestHeaderApiModel header,
            string continuationToken, IEnumerable<HistoricEventApiModel> returning,
            CancellationToken ct = default) {
            while (continuationToken != null) {
                var response = await client.HistoryReadEventsNextAsync(twinId,
                    new HistoryReadNextRequestApiModel {
                        ContinuationToken = continuationToken,
                        Header = header
                    }, ct).ConfigureAwait(false);
                continuationToken = response.ContinuationToken;
                returning = returning.Concat(response.History);
            }
            return returning;
        }

    }
}
