// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Storage {
    using Microsoft.Azure.IIoT.Platform.Twin.Storage.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Database twin repository
    /// </summary>
    public class TwinDatabase : ITwinRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        public TwinDatabase(ICollectionFactory databaseServer) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            _documents = databaseServer.OpenAsync("twins").Result;
        }

        /// <inheritdoc/>
        public async Task<TwinInfoModel> AddAsync(TwinInfoModel twin,
            CancellationToken ct) {
            if (twin == null) {
                throw new ArgumentNullException(nameof(twin));
            }
            var presetId = twin.Id;
            while (true) {
                if (!string.IsNullOrEmpty(twin.Id)) {
                    var document = await _documents.FindAsync<TwinDocument>(
                        twin.Id, ct: ct).ConfigureAwait(false);
                    if (document != null) {
                        throw new ResourceConflictException(
                            $"Twin {twin.Id} already exists.");
                    }
                }
                else {
                    twin.Id = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(twin.ToDocumentModel(),
                        ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel(result.Etag);
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    twin.Id = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TwinInfoModel> AddOrUpdateAsync(string twinId,
            Func<TwinInfoModel, Task<TwinInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            while (true) {
                var document = await _documents.FindAsync<TwinDocument>(
                    twinId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToServiceModel(document.Etag);
                var twin = await predicate(updateOrAdd).ConfigureAwait(false);
                if (twin == null) {
                    return updateOrAdd;
                }
                twin.Id = twinId;
                var updated = twin.ToDocumentModel();
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated,
                            ct: ct).ConfigureAwait(false);
                        return result.Value.ToServiceModel(result.Etag);
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
                try {
                    var result = await _documents.ReplaceAsync(document, updated,
                        ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel(result.Etag);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TwinInfoModel> UpdateAsync(string twinId,
            Func<TwinInfoModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            while (true) {
                var document = await _documents.FindAsync<TwinDocument>(
                    twinId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("twin not found");
                }
                var twin = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(twin).ConfigureAwait(false)) {
                    return twin;
                }
                twinId = twin.Id;
                var updated = twin.ToDocumentModel();
                try {
                    var result = await _documents.ReplaceAsync(document,
                        updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel(result.Etag);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<TwinInfoModel> FindAsync(string twinId, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            var document = await _documents.FindAsync<TwinDocument>(
                twinId, ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToServiceModel(document.Etag);
        }

        /// <inheritdoc/>
        public async Task<TwinInfoListModel> QueryAsync(TwinInfoQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<TwinDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<TwinDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new TwinInfoListModel {
                    Items = new List<TwinInfoModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new TwinInfoListModel {
                ContinuationToken = results.ContinuationToken,
                Items = documents.Select(r => r.Value.ToServiceModel(r.Etag)).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<TwinInfoModel> DeleteAsync(string twinId,
            Func<TwinInfoModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            while (true) {
                var document = await _documents.FindAsync<TwinDocument>(twinId,
                    ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var twin = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(twin).ConfigureAwait(false)) {
                    return twin;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return twin;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string twinId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(twinId)) {
                throw new ArgumentNullException(nameof(twinId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<TwinDocument>(
                twinId, null, generationId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<TwinDocument>> CreateQuery(
            IQuery<TwinDocument> query, TwinInfoQueryModel filter) {
            if (filter != null) {
                if (filter?.EndpointId != null) {
                    // If discoverer provided, include it in search
                    query = query.Where(x => x.EndpointId != null);
                    query = query.Where(x => x.EndpointId.Equals(
                        filter.EndpointId, StringComparison.OrdinalIgnoreCase));
                }
                if (filter?.Credential != null) {
                    // If SecurityMode provided, include it in search
                    query = query.Where(x => x.CredentialType == filter.Credential.Value);
                }
                if (filter?.State != null) {
                    query = query.Where(x => x.ConnectionState == filter.State.Value);
                }
            }
            query = query.Where(x => x.ClassType == IdentityType.Twin);
            return query.GetResults();
        }

        private readonly IDocumentCollection _documents;
    }
}