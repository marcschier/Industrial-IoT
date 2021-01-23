// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Storage {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Storage.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Database group repository
    /// </summary>
    public class WriterGroupDatabase : IWriterGroupRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        public WriterGroupDatabase(ICollectionFactory databaseServer) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            _documents = databaseServer.OpenAsync("publisher").Result;
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoModel> AddAsync(WriterGroupInfoModel writerGroup,
            CancellationToken ct) {
            if (writerGroup == null) {
                throw new ArgumentNullException(nameof(writerGroup));
            }
            var presetId = writerGroup.WriterGroupId;
            while (true) {
                if (!string.IsNullOrEmpty(writerGroup.WriterGroupId)) {
                    var document = await _documents.FindAsync<WriterGroupDocument>(
                        writerGroup.WriterGroupId, ct: ct).ConfigureAwait(false);
                    if (document != null) {
                        throw new ResourceConflictException(
                            $"Writer Group {writerGroup.WriterGroupId} already exists.");
                    }
                }
                else {
                    writerGroup.WriterGroupId = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(writerGroup.ToDocumentModel(), ct: ct).ConfigureAwait(false);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    writerGroup.WriterGroupId = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoModel> AddOrUpdateAsync(string writerGroupId,
            Func<WriterGroupInfoModel, Task<WriterGroupInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            while (true) {
                var document = await _documents.FindAsync<WriterGroupDocument>(writerGroupId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToFrameworkModel();
                var group = await predicate(updateOrAdd).ConfigureAwait(false);
                if (group == null) {
                    return updateOrAdd;
                }
                group.WriterGroupId = writerGroupId;
                var updated = group.ToDocumentModel();
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated, ct: ct).ConfigureAwait(false);
                        return result.Value.ToFrameworkModel();
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoModel> UpdateAsync(string writerGroupId,
            Func<WriterGroupInfoModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            while (true) {
                var document = await _documents.FindAsync<WriterGroupDocument>(writerGroupId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Writer group not found");
                }
                var group = document.Value.ToFrameworkModel();
                if (!await predicate(group).ConfigureAwait(false)) {
                    return group;
                }
                group.WriterGroupId = writerGroupId;
                var updated = group.ToDocumentModel();
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoModel> FindAsync(string writerGroupId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            var document = await _documents.FindAsync<WriterGroupDocument>(
                writerGroupId, ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToFrameworkModel();
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoListModel> QueryAsync(WriterGroupInfoQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<WriterGroupDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<WriterGroupDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new WriterGroupInfoListModel {
                    WriterGroups = new List<WriterGroupInfoModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new WriterGroupInfoListModel {
                ContinuationToken = results.ContinuationToken,
                WriterGroups = documents.Select(r => r.Value.ToFrameworkModel()).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<WriterGroupInfoModel> DeleteAsync(string writerGroupId,
            Func<WriterGroupInfoModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            while (true) {
                var document = await _documents.FindAsync<WriterGroupDocument>(
                    writerGroupId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var group = document.Value.ToFrameworkModel();
                if (!await predicate(group).ConfigureAwait(false)) {
                    return group;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return group;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string writerGroupId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(writerGroupId)) {
                throw new ArgumentNullException(nameof(writerGroupId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<WriterGroupDocument>(writerGroupId, null,
                generationId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<WriterGroupDocument>> CreateQuery(
            IQuery<WriterGroupDocument> query, WriterGroupInfoQueryModel filter) {
            if (filter != null) {
                if (filter.GroupVersion != null) {
                    query = query.Where(x => x.GroupVersion == filter.GroupVersion.Value);
                }
                if (filter.Encoding != null) {
                    query = query.Where(x => x.MessageEncoding == filter.Encoding.Value);
                }
                if (filter.Priority != null) {
                    query = query.Where(x => x.Priority == filter.Priority.Value);
                }
                if (filter.State != null) {
                    query = query.Where(x => x.LastState == filter.State.Value);
                }
                if (filter.Name != null) {
                    query = query.Where(x => x.Name == filter.Name);
                }
            }
            query = query.Where(x => x.ClassType == IdentityType.WriterGroup);
            return query.GetResults();
        }

        private readonly IDocumentCollection _documents;
    }
}