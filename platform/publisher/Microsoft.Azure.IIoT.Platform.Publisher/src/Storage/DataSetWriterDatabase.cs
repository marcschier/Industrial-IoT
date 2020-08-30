﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Storage.Default {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Database writer repository
    /// </summary>
    public class DataSetWriterDatabase : IDataSetWriterRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="config"></param>
        public DataSetWriterDatabase(IDatabaseServer databaseServer, IItemContainerConfig config) {
            var dbs = databaseServer.OpenAsync(config.DatabaseName).Result;
            _documents = dbs.OpenContainerAsync(config.ContainerName ?? "publisher").Result;
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoModel> AddAsync(DataSetWriterInfoModel writer,
            CancellationToken ct) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }
            var presetId = writer.DataSetWriterId;
            while (true) {
                if (!string.IsNullOrEmpty(writer.DataSetWriterId)) {
                    var document = await _documents.FindAsync<DataSetWriterDocument>(
                        writer.DataSetWriterId, ct);
                    if (document != null) {
                        throw new ResourceConflictException(
                            $"Dataset Writer {writer.DataSetWriterId} already exists.");
                    }
                }
                else {
                    writer.DataSetWriterId = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(writer.ToDocumentModel(), ct);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    writer.DataSetWriterId = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoModel> AddOrUpdateAsync(string writerId,
            Func<DataSetWriterInfoModel, Task<DataSetWriterInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerId)) {
                throw new ArgumentNullException(nameof(writerId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetWriterDocument>(writerId, ct);
                var updateOrAdd = document?.Value.ToFrameworkModel();
                var writer = await predicate(updateOrAdd);
                if (writer == null) {
                    return updateOrAdd;
                }
                writer.DataSetWriterId = writerId;
                var updated = writer.ToDocumentModel();
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated, ct);
                        return result.Value.ToFrameworkModel();
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoModel> UpdateAsync(string writerId,
            Func<DataSetWriterInfoModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(writerId)) {
                throw new ArgumentNullException(nameof(writerId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetWriterDocument>(writerId, ct);
                if (document == null) {
                    throw new ResourceNotFoundException("Dataset Writer not found");
                }
                var writer = document.Value.ToFrameworkModel();
                if (!await predicate(writer)) {
                    return writer;
                }
                writer.DataSetWriterId = writerId;
                var updated = writer.ToDocumentModel();
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct);
                    return result.Value.ToFrameworkModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoModel> FindAsync(string writerId, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerId)) {
                throw new ArgumentNullException(nameof(writerId));
            }
            var document = await _documents.FindAsync<DataSetWriterDocument>(writerId, ct);
            if (document == null) {
                return null;
            }
            return document.Value.ToFrameworkModel();
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoListModel> QueryAsync(DataSetWriterInfoQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<DataSetWriterDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<DataSetWriterDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new DataSetWriterInfoListModel {
                    DataSetWriters = new List<DataSetWriterInfoModel>()
                };
            }
            var documents = await results.ReadAsync(ct);
            return new DataSetWriterInfoListModel {
                ContinuationToken = results.ContinuationToken,
                DataSetWriters = documents.Select(r => r.Value.ToFrameworkModel()).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<DataSetWriterInfoModel> DeleteAsync(string writerId,
            Func<DataSetWriterInfoModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(writerId)) {
                throw new ArgumentNullException(nameof(writerId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetWriterDocument>(
                    writerId);
                if (document == null) {
                    return null;
                }
                var writer = document.Value.ToFrameworkModel();
                if (!await predicate(writer)) {
                    return writer;
                }
                try {
                    await _documents.DeleteAsync(document, ct);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return writer;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string writerId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(writerId)) {
                throw new ArgumentNullException(nameof(writerId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<DataSetWriterDocument>(writerId, ct, null, generationId);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<DataSetWriterDocument>> CreateQuery(
            IQuery<DataSetWriterDocument> query, DataSetWriterInfoQueryModel filter) {
            if (filter?.WriterGroupId != null) {
                query = query.Where(x => x.WriterGroupId == filter.WriterGroupId);
            }
            if (filter?.EndpointId != null) {
                query = query.Where(x => x.EndpointId == filter.EndpointId);
            }
            if (filter?.DataSetName != null) {
                query = query.Where(x => x.DataSetName == filter.DataSetName);
            }
            if (filter?.ExcludeDisabled == true) {
                query = query.Where(x => x.IsDisabled == false);
            }
            query = query.Where(x => x.ClassType == DataSetWriterDocument.ClassTypeName);
            return query.GetResults();
        }

        private readonly IItemContainer _documents;
    }
}