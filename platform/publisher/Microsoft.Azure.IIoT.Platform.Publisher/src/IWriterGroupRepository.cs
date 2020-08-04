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
    /// Writer group repository
    /// </summary>
    public interface IWriterGroupRepository {

        /// <summary>
        /// List writer groups
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuationToken"></param>
        /// <param name="maxResults"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriterGroupInfoListModel> QueryAsync(
            WriterGroupInfoQueryModel query = null,
            string continuationToken = null, int? maxResults = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get dataset writer group by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriterGroupInfoModel> FindAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Add new writer group to repository.
        /// The created writer group is returned.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ct"></param>
        /// <returns>The newly created writer</returns>
        Task<WriterGroupInfoModel> AddAsync(WriterGroupInfoModel writer,
            CancellationToken ct = default);

        /// <summary>
        /// Add a new one or update existing writer group
        /// </summary>
        /// <param name="id">writer group to create or update
        /// </param>
        /// <param name="predicate">receives existing writer group or
        /// null if not exists, return null to cancel.
        /// </param>
        /// <param name="ct"></param>
        /// <returns>The existing or udpated writer group</returns>
        Task<WriterGroupInfoModel> AddOrUpdateAsync(string id,
             Func<WriterGroupInfoModel, Task<WriterGroupInfoModel>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Update writer group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriterGroupInfoModel> UpdateAsync(string id,
             Func<WriterGroupInfoModel, Task<bool>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Delete writer group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WriterGroupInfoModel> DeleteAsync(string id,
            Func<WriterGroupInfoModel, Task<bool>> predicate,
            CancellationToken ct = default);

        /// <summary>
        /// Delete writer group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string id, string generationId,
            CancellationToken ct = default);
    }
}