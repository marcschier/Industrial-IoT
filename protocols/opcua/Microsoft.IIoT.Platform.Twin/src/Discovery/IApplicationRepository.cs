// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Application repository - used by application registry to
    /// store application objects.
    /// </summary>
    public interface IApplicationRepository {

        /// <summary>
        /// Query applications
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuationToken"></param>
        /// <param name="maxResults"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoListModel> QueryAsync(
            ApplicationInfoQueryModel query = null,
            string continuationToken = null, int? maxResults = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get application by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoModel> FindAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Add new application to repository.
        /// The created application is returned.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="ct"></param>
        /// <returns>The newly created writer</returns>
        Task<ApplicationInfoModel> AddAsync(ApplicationInfoModel application,
            CancellationToken ct = default);

        /// <summary>
        /// Add a new one or update existing application
        /// </summary>
        /// <param name="id">application to create or update
        /// </param>
        /// <param name="predicate">receives existing application or
        /// null if not exists, return null to cancel.
        /// </param>
        /// <param name="ct"></param>
        /// <returns>The existing or udpated application</returns>
        Task<ApplicationInfoModel> AddOrUpdateAsync(string id,
             Func<ApplicationInfoModel, Task<ApplicationInfoModel>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Update application
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoModel> UpdateAsync(string id,
             Func<ApplicationInfoModel, Task<bool>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Delete application
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ApplicationInfoModel> DeleteAsync(string id,
            Func<ApplicationInfoModel, Task<bool>> predicate,
            CancellationToken ct = default);

        /// <summary>
        /// Delete application
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string id, string generationId,
            CancellationToken ct = default);
    }
}