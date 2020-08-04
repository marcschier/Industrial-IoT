// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// DataSet writer repository
    /// </summary>
    public interface IDataSetWriterRepository {

        /// <summary>
        /// List writers
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuationToken"></param>
        /// <param name="maxResults"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoListModel> QueryAsync(
            DataSetWriterInfoQueryModel query = null,
            string continuationToken = null, int? maxResults = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get dataset writer by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoModel> FindAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Add new writer to repository. The created writer is
        /// returned.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ct"></param>
        /// <returns>The newly created writer</returns>
        Task<DataSetWriterInfoModel> AddAsync(DataSetWriterInfoModel writer,
            CancellationToken ct = default);

        /// <summary>
        /// Add a new one or update existing writer
        /// </summary>
        /// <param name="id">writer to create or update
        /// </param>
        /// <param name="predicate">receives existing writer or
        /// null if not exists, return null to cancel.
        /// </param>
        /// <param name="ct"></param>
        /// <returns>The existing or udpated writer</returns>
        Task<DataSetWriterInfoModel> AddOrUpdateAsync(string id,
             Func<DataSetWriterInfoModel, Task<DataSetWriterInfoModel>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Update writer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoModel> UpdateAsync(string id,
             Func<DataSetWriterInfoModel, Task<bool>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Delete writer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DataSetWriterInfoModel> DeleteAsync(string id,
            Func<DataSetWriterInfoModel, Task<bool>> predicate,
            CancellationToken ct = default);

        /// <summary>
        /// Delete writer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string id, string generationId,
            CancellationToken ct = default);
    }
}