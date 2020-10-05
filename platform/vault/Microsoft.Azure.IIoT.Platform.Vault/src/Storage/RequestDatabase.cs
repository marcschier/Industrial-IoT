// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Storage {
    using Microsoft.Azure.IIoT.Platform.Vault.Storage.Models;
    using Microsoft.Azure.IIoT.Platform.Vault.Models;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Cosmos db certificate request database
    /// </summary>
    public sealed class RequestDatabase : IRequestRepository {

        /// <summary>
        /// Create certificate request
        /// </summary>
        /// <param name="db"></param>
        public RequestDatabase(IItemContainerFactory db) {
            if (db == null) {
                throw new ArgumentNullException(nameof(db));
            }

            _requests = db.OpenAsync("requests").Result;
            _index = new ContainerIndex(db, _requests.Name);
        }

        /// <inheritdoc/>
        public async Task<CertificateRequestModel> AddAsync(
            CertificateRequestModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var recordId = await _index.AllocateAsync(ct).ConfigureAwait(false);
            while (true) {
                request.Index = recordId;
                request.Record.State = CertificateRequestState.New;
                request.Record.RequestId = "req" + Guid.NewGuid();
                try {
                    var result = await _requests.AddAsync(request.ToDocument(), ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel();
                }
                catch (ResourceConflictException) {
                    continue;
                }
                catch {
                    await Try.Async(() => _index.FreeAsync(recordId)).ConfigureAwait(false);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<CertificateRequestModel> UpdateAsync(string requestId,
            Func<CertificateRequestModel, bool> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            while (true) {
                var document = await _requests.FindAsync<RequestDocument>(requestId, 
                    ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Request not found");
                }
                var request = document.Value.ToServiceModel();
                if (!predicate(request)) {
                    return request;
                }
                var updated = request.ToDocument(document.Value.ETag);
                try {
                    var result = await _requests.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel();
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<CertificateRequestModel> DeleteAsync(string requestId,
            Func<CertificateRequestModel, bool> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            while (true) {
                var document = await _requests.FindAsync<RequestDocument>(requestId, 
                    ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var request = document.Value.ToServiceModel();
                if (!predicate(request)) {
                    return request;
                }
                try {
                    await _requests.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                    await Try.Async(() => _index.FreeAsync(document.Value.Index)).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return request;
            }
        }

        /// <inheritdoc/>
        public async Task<CertificateRequestModel> FindAsync(string requestId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(requestId)) {
                throw new ArgumentNullException(nameof(requestId));
            }
            var document = await _requests.FindAsync<RequestDocument>(
                requestId, ct: ct).ConfigureAwait(false);
            if (document == null) {
                throw new ResourceNotFoundException("Request not found");
            }
            return document.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<CertificateRequestListModel> QueryAsync(
            CertificateRequestQueryRequestModel query, string nextPageLink,
            int? maxResults, CancellationToken ct) {
            var results = nextPageLink != null ?
                _requests.ContinueQuery<RequestDocument>(nextPageLink, maxResults) :
                CreateQuery(_requests.CreateQuery<RequestDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new CertificateRequestListModel {
                    Requests = new List<CertificateRequestModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new CertificateRequestListModel {
                NextPageLink = results.ContinuationToken,
                Requests = documents.Select(r => r.Value.ToServiceModel()).ToList()
            };
        }

        /// <summary>
        /// Create query string from parameters
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<RequestDocument>> CreateQuery(
            IQuery<RequestDocument> query, CertificateRequestQueryRequestModel filter) {

            if (filter?.State != null) {
                query = query.Where(x => x.State == filter.State);
            }
            if (filter?.EntityId != null) {
                query = query.Where(x => x.Entity.Id == filter.EntityId);
            }
            query = query.Where(x => x.ClassType == RequestDocument.ClassTypeName);
            return query.GetResults();
        }

        private readonly IItemContainer _requests;
        private readonly IContainerIndex _index;
    }
}
