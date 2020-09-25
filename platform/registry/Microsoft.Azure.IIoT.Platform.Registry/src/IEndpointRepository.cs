// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Endpoint repository
    /// </summary>
    public interface IEndpointRepository {

        /// <summary>
        /// Query endpoints
        /// </summary>
        /// <param name="query"></param>
        /// <param name="continuationToken"></param>
        /// <param name="maxResults"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoListModel> QueryAsync(
            EndpointRegistrationQueryModel query = null,
            string continuationToken = null, int? maxResults = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get endpoint by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoModel> FindAsync(string id,
            CancellationToken ct = default);

        /// <summary>
        /// Add new endpoint to repository.
        /// The created endpoint is returned.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ct"></param>
        /// <returns>The newly created writer</returns>
        Task<EndpointInfoModel> AddAsync(EndpointInfoModel writer,
            CancellationToken ct = default);

        /// <summary>
        /// Add a new one or update existing endpoint
        /// </summary>
        /// <param name="id">endpoint to create or update
        /// </param>
        /// <param name="predicate">receives existing endpoint or
        /// null if not exists, return null to cancel.
        /// </param>
        /// <param name="ct"></param>
        /// <returns>The existing or udpated endpoint</returns>
        Task<EndpointInfoModel> AddOrUpdateAsync(string id,
             Func<EndpointInfoModel, Task<EndpointInfoModel>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Update endpoint
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoModel> UpdateAsync(string id,
             Func<EndpointInfoModel, Task<bool>> predicate,
             CancellationToken ct = default);

        /// <summary>
        /// Delete endpoint
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EndpointInfoModel> DeleteAsync(string id,
            Func<EndpointInfoModel, Task<bool>> predicate,
            CancellationToken ct = default);

        /// <summary>
        /// Delete endpoint
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string id, string generationId,
            CancellationToken ct = default);
    }
}