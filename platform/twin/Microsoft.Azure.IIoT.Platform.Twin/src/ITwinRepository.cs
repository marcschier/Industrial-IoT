// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Twin repository
    /// </summary>
    public interface ITwinRepository {

        /// <summary>
        /// Query twins
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuationToken"></param>
        /// <param name="maxResults"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoListModel> QueryAsync(
            TwinInfoQueryModel query = null,
            string continuationToken = null, int? maxResults = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get twin by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoModel> FindAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Add new twin to repository.
        /// The created twin is returned.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ct"></param>
        /// <returns>The newly created writer</returns>
        Task<TwinInfoModel> AddAsync(TwinInfoModel writer,
            CancellationToken ct = default);

        /// <summary>
        /// Add a new one or update existing twin
        /// </summary>
        /// <param name="id">twin to create or update
        /// </param>
        /// <param name="predicate">receives existing twin or
        /// null if not exists, return null to cancel.
        /// </param>
        /// <param name="ct"></param>
        /// <returns>The existing or udpated twin</returns>
        Task<TwinInfoModel> AddOrUpdateAsync(string id,
             Func<TwinInfoModel, Task<TwinInfoModel>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Update twin
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoModel> UpdateAsync(string id,
             Func<TwinInfoModel, Task<bool>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Delete twin
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TwinInfoModel> DeleteAsync(string id,
            Func<TwinInfoModel, Task<bool>> predicate,
            CancellationToken ct = default);

        /// <summary>
        /// Delete twin
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string id, string generationId,
            CancellationToken ct = default);
    }
}