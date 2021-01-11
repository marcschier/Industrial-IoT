// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Storage {
    using Microsoft.IIoT.Platform.Publisher.Storage.Models;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Dataset repository
    /// </summary>
    public class DataSetEntityDatabase : IDataSetEntityRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        public DataSetEntityDatabase(ICollectionFactory databaseServer) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            _documents = databaseServer.OpenAsync("publisher").Result;
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetEventsModel> AddEventDataSetAsync(string dataSetWriterId,
            PublishedDataSetEventsModel item, CancellationToken ct) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            item.Id = DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId);
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    item.Id, ct: ct).ConfigureAwait(false);
                if (document != null) {
                    throw new ResourceConflictException($"Events {item.Id} already exists.");
                }
                try {
                    var result = await _documents.AddAsync(
                        item.ToDocumentModel(dataSetWriterId), ct: ct).ConfigureAwait(false);
                    return result.Value.ToEventDataSetModel();
                }
                catch (ResourceConflictException) {
                    // Try again
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetEventsModel> AddOrUpdateEventDataSetAsync(string dataSetWriterId,
            Func<PublishedDataSetEventsModel, Task<PublishedDataSetEventsModel>> predicate,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var eventsId = DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId);
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    eventsId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToEventDataSetModel();
                var item = await predicate(updateOrAdd).ConfigureAwait(false);
                if (item == null) {
                    return updateOrAdd;
                }
                var updated = item.ToDocumentModel(dataSetWriterId);
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated, ct: ct).ConfigureAwait(false);
                        return result.Value.ToEventDataSetModel();
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToEventDataSetModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetEventsModel> UpdateEventDataSetAsync(string dataSetWriterId,
            Func<PublishedDataSetEventsModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var eventsId = DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId);
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    eventsId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Events not found");
                }
                var item = document.Value.ToEventDataSetModel();
                if (!await predicate(item).ConfigureAwait(false)) {
                    return item;
                }
                var updated = item.ToDocumentModel(dataSetWriterId);
                try {
                    var result = await _documents.ReplaceAsync(document,
                        updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToEventDataSetModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetEventsModel> FindEventDataSetAsync(string dataSetWriterId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var document = await _documents.FindAsync<DataSetEntityDocument>(
                DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId), ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToEventDataSetModel();
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetEventsModel> DeleteEventDataSetAsync(string dataSetWriterId,
            Func<PublishedDataSetEventsModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId),
                        ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var dataset = document.Value.ToEventDataSetModel();
                if (!await predicate(dataset).ConfigureAwait(false)) {
                    return dataset;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return dataset;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteEventDataSetAsync(string dataSetWriterId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<DataSetEntityDocument>(
                DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId), null, generationId,
                    ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableModel> AddDataSetVariableAsync(string dataSetWriterId,
            PublishedDataSetVariableModel item, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            var presetId = item.Id;
            while (true) {
                if (!string.IsNullOrEmpty(item.Id)) {
                    var document = await _documents.FindAsync<DataSetEntityDocument>(
                        DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, item.Id),
                            ct: ct).ConfigureAwait(false);
                    if (document != null) {
                        throw new ResourceConflictException($"Variable {item.Id} already exists.");
                    }
                }
                else {
                    item.Id = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(item.ToDocumentModel(dataSetWriterId),
                            ct: ct).ConfigureAwait(false);
                    return result.Value.ToDataSetVariableModel();
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    item.Id = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableModel> AddOrUpdateDataSetVariableAsync(
            string dataSetWriterId, string variableId,
            Func<PublishedDataSetVariableModel, Task<PublishedDataSetVariableModel>> predicate,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, variableId), ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToDataSetVariableModel();
                var variable = await predicate(updateOrAdd).ConfigureAwait(false);
                if (variable == null) {
                    return updateOrAdd;
                }
                variable.Id = variableId;
                var updated = variable.ToDocumentModel(dataSetWriterId);
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated, ct: ct).ConfigureAwait(false);
                        return result.Value.ToDataSetVariableModel();
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToDataSetVariableModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableModel> UpdateDataSetVariableAsync(string dataSetWriterId,
            string variableId, Func<PublishedDataSetVariableModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, variableId), ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Variable not found");
                }
                var variable = document.Value.ToDataSetVariableModel();
                if (!await predicate(variable).ConfigureAwait(false)) {
                    return variable;
                }
                variable.Id = variableId;
                var updated = variable.ToDocumentModel(dataSetWriterId);
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToDataSetVariableModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableModel> FindDataSetVariableAsync(string dataSetWriterId,
            string variableId, CancellationToken ct) {
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var document = await _documents.FindAsync<DataSetEntityDocument>(
                DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, variableId), ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToDataSetVariableModel();
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableModel> DeleteDataSetVariableAsync(string dataSetWriterId,
            string variableId, Func<PublishedDataSetVariableModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            while (true) {
                var document = await _documents.FindAsync<DataSetEntityDocument>(
                    DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, variableId),
                        ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var variable = document.Value.ToDataSetVariableModel();
                if (!await predicate(variable).ConfigureAwait(false)) {
                    return variable;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return variable;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteDataSetVariableAsync(string dataSetWriterId, string variableId,
            string generationId, CancellationToken ct) {
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _documents.DeleteAsync<DataSetEntityDocument>(
                DataSetEntityDocumentEx.GetDocumentId(dataSetWriterId, variableId),
                null, generationId, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeleteDataSetAsync(string dataSetWriterId, CancellationToken ct) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var results = _documents.CreateQuery<DataSetEntityDocument>()
                .Where(x => x.DataSetWriterId == dataSetWriterId)
                .Where(x => x.ClassType == IdentityType.DataSetEntity)
                .GetResults();
            await results.ForEachAsync(d =>
                _documents.DeleteAsync<DataSetEntityDocument>(d.Id, ct: ct), ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<PublishedDataSetVariableListModel> QueryDataSetVariablesAsync(
            string dataSetWriterId, PublishedDataSetVariableQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<DataSetEntityDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<DataSetEntityDocument>(maxResults),
                    dataSetWriterId, query);
            if (!results.HasMore()) {
                return new PublishedDataSetVariableListModel {
                    Variables = new List<PublishedDataSetVariableModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new PublishedDataSetVariableListModel {
                ContinuationToken = results.ContinuationToken,
                Variables = documents.Select(r => r.Value.ToDataSetVariableModel()).ToList()
            };
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<DataSetEntityDocument>> CreateQuery(
            IQuery<DataSetEntityDocument> query, string dataSetWriterId,
            PublishedDataSetVariableQueryModel filter) {
            if (!string.IsNullOrEmpty(dataSetWriterId)) {
                query = query.Where(x => x.DataSetWriterId == dataSetWriterId);
            }
            if (filter != null) {
                if (filter.Attribute != null) {
                    query = query.Where(x => x.Attribute == filter.Attribute.Value);
                }
                if (filter.PublishedVariableDisplayName != null) {
                    query = query.Where(x => x.DisplayName == filter.PublishedVariableDisplayName);
                }
                if (filter.PublishedVariableNodeId != null) {
                    query = query.Where(x => x.NodeId == filter.PublishedVariableNodeId);
                }
            }
            query = query
                .Where(x => x.ClassType == IdentityType.DataSetEntity)
                .OrderBy(x => x.NodeId);
            return query.GetResults();
        }

        private readonly IDocumentCollection _documents;
    }
}