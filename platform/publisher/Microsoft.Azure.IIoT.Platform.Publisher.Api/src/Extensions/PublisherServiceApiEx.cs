// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for vault client to adapt to v1.
    /// </summary>
    public static class PublisherServiceApiEx {

        /// <summary>
        /// List all groups
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<WriterGroupInfoApiModel>> ListAllWriterGroupsAsync(
            this IPublisherServiceApi service, CancellationToken ct = default) {
            var groups = new List<WriterGroupInfoApiModel>();
            var result = await service.ListWriterGroupsAsync(null, null, ct);
            groups.AddRange(result.WriterGroups);
            while (result.ContinuationToken != null) {
                result = await service.ListWriterGroupsAsync(result.ContinuationToken,
                    null, ct);
                groups.AddRange(result.WriterGroups);
            }
            return groups;
        }

        /// <summary>
        /// Find groups
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<WriterGroupInfoApiModel>> QueryAllWriterGroupsAsync(
            this IPublisherServiceApi service, WriterGroupInfoQueryApiModel query,
            CancellationToken ct = default) {
            var groups = new List<WriterGroupInfoApiModel>();
            var result = await service.QueryWriterGroupsAsync(query, null, ct);
            groups.AddRange(result.WriterGroups);
            while (result.ContinuationToken != null) {
                result = await service.ListWriterGroupsAsync(result.ContinuationToken,
                    null, ct);
                groups.AddRange(result.WriterGroups);
            }
            return groups;
        }

        /// <summary>
        /// List all writers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<DataSetWriterInfoApiModel>> ListAllDataSetWritersAsync(
            this IPublisherServiceApi service, CancellationToken ct = default) {
            var writers = new List<DataSetWriterInfoApiModel>();
            var result = await service.ListDataSetWritersAsync(null, null, ct);
            writers.AddRange(result.DataSetWriters);
            while (result.ContinuationToken != null) {
                result = await service.ListDataSetWritersAsync(result.ContinuationToken,
                    null, ct);
                writers.AddRange(result.DataSetWriters);
            }
            return writers;
        }

        /// <summary>
        /// Find writers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<DataSetWriterInfoApiModel>> QueryAllDataSetWritersAsync(
            this IPublisherServiceApi service, DataSetWriterInfoQueryApiModel query,
            CancellationToken ct = default) {
            var writers = new List<DataSetWriterInfoApiModel>();
            var result = await service.QueryDataSetWritersAsync(query, null, ct);
            writers.AddRange(result.DataSetWriters);
            while (result.ContinuationToken != null) {
                result = await service.ListDataSetWritersAsync(result.ContinuationToken,
                    null, ct);
                writers.AddRange(result.DataSetWriters);
            }
            return writers;
        }

        /// <summary>
        /// List all variables
        /// </summary>
        /// <param name="service"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PublishedDataSetVariableApiModel>> ListAllDataSetVariablesAsync(
            this IPublisherServiceApi service, string dataSetWriterId, CancellationToken ct = default) {
            var writers = new List<PublishedDataSetVariableApiModel>();
            var result = await service.ListDataSetVariablesAsync(dataSetWriterId, null, null, ct);
            writers.AddRange(result.Variables);
            while (result.ContinuationToken != null) {
                result = await service.ListDataSetVariablesAsync(dataSetWriterId, result.ContinuationToken,
                    null, ct);
                writers.AddRange(result.Variables);
            }
            return writers;
        }

        /// <summary>
        /// Find writers
        /// </summary>
        /// <param name="service"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PublishedDataSetVariableApiModel>> QueryAllDataSetVariablesAsync(
            this IPublisherServiceApi service, string dataSetWriterId, PublishedDataSetVariableQueryApiModel query,
            CancellationToken ct = default) {
            var writers = new List<PublishedDataSetVariableApiModel>();
            var result = await service.QueryDataSetVariablesAsync(dataSetWriterId, query, null, ct);
            writers.AddRange(result.Variables);
            while (result.ContinuationToken != null) {
                result = await service.ListDataSetVariablesAsync(dataSetWriterId, result.ContinuationToken,
                    null, ct);
                writers.AddRange(result.Variables);
            }
            return writers;
        }
    }
}
