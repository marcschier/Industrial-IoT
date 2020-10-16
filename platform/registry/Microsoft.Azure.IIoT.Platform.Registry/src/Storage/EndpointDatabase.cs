// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Storage {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Hub;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Database endpoint repository
    /// </summary>
    public class EndpointDatabase : IEndpointRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="config"></param>
        public EndpointDatabase(IDatabaseServer databaseServer, IItemContainerConfig config) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            var dbs = databaseServer.OpenAsync(config.DatabaseName).Result;
            _documents = dbs.OpenContainerAsync(config.ContainerName ?? "registry").Result;
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> AddAsync(EndpointInfoModel endpoint,
            CancellationToken ct) {
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            endpoint = endpoint.SetEndpointId();
            var result = await _documents.AddAsync(
                endpoint.ToDocumentModel(), ct: ct).ConfigureAwait(false);
            return result.Value.ToServiceModel(result.Etag);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> AddOrUpdateAsync(string endpointId,
            Func<EndpointInfoModel, Task<EndpointInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            while (true) {
                var document = await _documents.FindAsync<EndpointDocument>(
                    endpointId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToServiceModel(document.Etag);
                var endpoint = await predicate(updateOrAdd).ConfigureAwait(false);
                if (endpoint == null) {
                    return updateOrAdd;
                }
                endpoint.SetEndpointId();
                endpointId = endpoint.Id;
                var updated = endpoint.ToDocumentModel();
                if (document == null) {
                    try {
                        // Add document
                        var result = await _documents.AddAsync(updated, ct: ct).ConfigureAwait(false);
                        return result.Value.ToServiceModel(result.Etag);
                    }
                    catch (ResourceConflictException) {
                        // Conflict - try update now
                        continue;
                    }
                }
                // Try replacing
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
        public async Task<EndpointInfoModel> UpdateAsync(string endpointId,
            Func<EndpointInfoModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            while (true) {
                var document = await _documents.FindAsync<EndpointDocument>(
                    endpointId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Endpoint not found");
                }
                var endpoint = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(endpoint).ConfigureAwait(false)) {
                    return endpoint;
                }
                endpoint.SetEndpointId();
                endpointId = endpoint.Id;
                var updated = endpoint.ToDocumentModel();
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
        public async Task<EndpointInfoModel> FindAsync(string endpointId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var document = await _documents.FindAsync<EndpointDocument>(
                endpointId, ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToServiceModel(document.Etag);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> QueryAsync(EndpointInfoQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<EndpointDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<EndpointDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new EndpointInfoListModel {
                    Items = new List<EndpointInfoModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new EndpointInfoListModel {
                ContinuationToken = results.ContinuationToken,
                Items = documents.Select(r => r.Value.ToServiceModel(r.Etag)).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> DeleteAsync(string endpointId,
            Func<EndpointInfoModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            while (true) {
                var document = await _documents.FindAsync<EndpointDocument>(endpointId,
                    ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var endpoint = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(endpoint).ConfigureAwait(false)) {
                    return null;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return endpoint;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string endpointId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<EndpointDocument>(
                endpointId, null, generationId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<EndpointDocument>> CreateQuery(
            IQuery<EndpointDocument> query, EndpointInfoQueryModel filter) {
            if (filter != null) {
                if (filter?.Visibility != null) {
                    // Scope to non deleted applications
                    query = query.Where(x => x.Visibility == filter.Visibility.Value);
                }
                if (filter?.Url != null) {
                    // If Url provided, include it in search
                    query = query.Where(x => x.EndpointUrl != null);
                    query = query.Where(x => x.EndpointUrl.Equals(filter.Url,
                        StringComparison.OrdinalIgnoreCase));
                }
                if (filter?.ApplicationId != null) {
                    // If application id provided, include it in search
                    query = query.Where(x => x.ApplicationId != null);
                    query = query.Where(x => x.ApplicationId.Equals(
                        filter.ApplicationId, StringComparison.OrdinalIgnoreCase));
                }
                if (filter?.DiscovererId != null) {
                    // If discoverer provided, include it in search
                    query = query.Where(x => x.DiscovererId != null);
                    query = query.Where(x => x.DiscovererId.Equals(
                        filter.DiscovererId, StringComparison.OrdinalIgnoreCase));
                }
                if (filter?.Certificate != null) {
                    // If cert thumbprint provided, include it in search
                    query = query.Where(x => x.Thumbprint != null);
                    query = query.Where(x => x.Thumbprint.Equals(filter.Certificate, 
                        StringComparison.Ordinal));
                }
                if (filter?.SecurityMode != null) {
                    // If SecurityMode provided, include it in search
                    query = query.Where(x => x.SecurityMode == filter.SecurityMode);
                }
                if (filter?.SecurityPolicy != null) {
                    // If SecurityPolicy uri provided, include it in search
                    query = query.Where(x => x.SecurityPolicy != null);
                    query = query.Where(x => x.SecurityPolicy.Equals(filter.SecurityPolicy,
                        StringComparison.Ordinal));
                }
                if (filter?.EndpointState != null) {
                    query = query.Where(x => x.EndpointState == filter.EndpointState.Value);
                }
                if (filter?.ActivationState != null) {
                    query = query.Where(x => x.ActivationState == filter.ActivationState.Value);
                }
            }
            query = query.Where(x => x.ClassType == IdentityType.Endpoint);
            return query.GetResults();
        }

        private readonly IItemContainer _documents;
    }
}