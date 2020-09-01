// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Agent.Framework.Storage.Database {
    using Microsoft.Azure.IIoT.Agent.Framework.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Database based worker registry
    /// </summary>
    public class WorkerDatabase : IWorkerRegistry, IWorkerRepository {

        /// <summary>
        /// Create worker registry
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="databaseRegistryConfig"></param>
        public WorkerDatabase(IDatabaseServer databaseServer, IWorkerDatabaseConfig databaseRegistryConfig) {
            var database = databaseServer.OpenAsync(databaseRegistryConfig.DatabaseName).Result;
            _documents = database.OpenContainerAsync(databaseRegistryConfig.ContainerName).Result;
        }

        /// <inheritdoc/>
        public async Task AddOrUpdate(WorkerHeartbeatModel workerHeartbeat,
            CancellationToken ct) {
            if (workerHeartbeat == null) {
                throw new ArgumentNullException(nameof(workerHeartbeat));
            }
            while (true) {
                var workerDocument = new WorkerDocument {
                    AgentId = workerHeartbeat.AgentId,
                    Id = workerHeartbeat.WorkerId,
                    WorkerStatus = workerHeartbeat.Status,
                    LastSeen = DateTime.UtcNow
                };
                var existing = await _documents.FindAsync<WorkerDocument>(
                    workerHeartbeat.WorkerId);
                if (existing != null) {
                    try {
                        workerDocument.ETag = existing.Etag;
                        workerDocument.Id = existing.Id;
                        await _documents.ReplaceAsync(existing, workerDocument);
                        return;
                    }
                    catch (ResourceOutOfDateException) {
                        continue; // try again refreshing the etag
                    }
                    catch (ResourceNotFoundException) {
                        continue;
                    }
                }
                try {
                    await _documents.AddAsync(workerDocument);
                    return;
                }
                catch (ResourceConflictException) {
                    // Try to update
                    continue;
                }
            }
        }
        /// <inheritdoc/>
        public async Task<WorkerInfoListModel> ListWorkersAsync(string continuationToken,
            int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<WorkerDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<WorkerDocument>(maxResults));
            if (!results.HasMore()) {
                return new WorkerInfoListModel();
            }
            var documents = await results.ReadAsync(ct);
            return new WorkerInfoListModel {
                ContinuationToken = results.ContinuationToken,
                Workers = documents.Select(r => r.Value.ToFrameworkModel()).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<WorkerInfoModel> GetWorkerAsync(string workerId, CancellationToken ct) {
            if (string.IsNullOrEmpty(workerId)) {
                throw new ArgumentNullException(nameof(workerId));
            }
            var document = await _documents.FindAsync<WorkerDocument>(workerId, ct);
            if (document == null) {
                throw new ResourceNotFoundException("Worker not found");
            }
            return document.Value.ToFrameworkModel();
        }

        /// <inheritdoc/>
        public async Task DeleteWorkerAsync(string workerId, CancellationToken ct) {
            if (string.IsNullOrEmpty(workerId)) {
                throw new ArgumentNullException(nameof(workerId));
            }
            await _documents.DeleteAsync<WorkerDocument>(workerId, ct);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<WorkerDocument>> CreateQuery(
            IQuery<WorkerDocument> query) {
            query = query.Where(x => x.ClassType == WorkerDocument.ClassTypeName);
            return query.GetResults();
        }

        private readonly IItemContainer _documents;
    }
}