// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Storage.Default {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Serializers;
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
        /// <param name="serializer"></param>
        /// <param name="config"></param>
        public EndpointDatabase(IDatabaseServer databaseServer, IJsonSerializer serializer,
            IItemContainerConfig config) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            var dbs = databaseServer.OpenAsync(config.DatabaseName).Result;
            _documents = dbs.OpenContainerAsync(config.ContainerName ?? "twin").Result;
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> AddAsync(EndpointInfoModel endpoint,
            CancellationToken ct) {
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var presetId = endpoint.Id;
            while (true) {
                if (!string.IsNullOrEmpty(endpoint.Id)) {
                    var document = await _documents.FindAsync<EndpointRegistration>(
                        endpoint.Id, ct: ct).ConfigureAwait(false);
                    if (document != null) {
                        throw new ResourceConflictException(
                            $"Writer Group {endpoint.Id} already exists.");
                    }
                }
                else {
                    endpoint.Id = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(
                        endpoint.ToEndpointRegistration(_serializer), ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel(result.Etag);
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    endpoint.Id = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> AddOrUpdateAsync(string endpointId,
            Func<EndpointInfoModel, Task<EndpointInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            while (true) {
                var document = await _documents.FindAsync<EndpointRegistration>(endpointId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToServiceModel(document.Etag);
                var endpoint = await predicate(updateOrAdd).ConfigureAwait(false);
                if (endpoint == null) {
                    return updateOrAdd;
                }
                endpoint.Id = endpointId;
                var updated = endpoint.ToEndpointRegistration(_serializer);
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
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
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
                var document = await _documents.FindAsync<EndpointRegistration>(endpointId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Endpoint not found");
                }
                var endpoint = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(endpoint).ConfigureAwait(false)) {
                    return endpoint;
                }
                endpoint.Id = endpointId;
                var updated = endpoint.ToEndpointRegistration(_serializer);
                try {
                    var result = await _documents.ReplaceAsync(document, updated, ct: ct).ConfigureAwait(false);
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
            var document = await _documents.FindAsync<EndpointRegistration>(
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
                _documents.ContinueQuery<EndpointRegistration>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<EndpointRegistration>(maxResults), query);
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
                var document = await _documents.FindAsync<EndpointRegistration>(endpointId,
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
            await _documents.DeleteAsync<EndpointRegistration>(
                endpointId, null, generationId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<EndpointRegistration>> CreateQuery(
            IQuery<EndpointRegistration> query, EndpointInfoQueryModel filter) {
            if (filter != null) {
                if (!(filter?.IncludeNotSeenSince ?? false)) {
                    // Scope to non deleted twins
                    query = query.Where(x => x.NotSeenSince == null);
                }
                if (filter?.Url != null) {
                    // If Url provided, include it in search
                    query = query.Where(x => x.EndpointUrlLC == filter.Url.ToLowerInvariant());
                }
                if (filter?.ApplicationId != null) {
                    // If application id provided, include it in search
                    query = query.Where(x => x.ApplicationId == filter.ApplicationId);
                }
                if (filter?.SupervisorId != null) {
                    // If supervisor provided, include it in search
                    query = query.Where(x => x.SupervisorId == filter.SupervisorId);
                }
                if (filter?.DiscovererId != null) {
                    // If discoverer provided, include it in search
                    query = query.Where(x => x.DiscovererId == filter.DiscovererId);
                }
                if (filter?.SiteOrGatewayId != null) {
                    // If site or gateway provided, include it in search
                    query = query.Where(x => x.SiteOrGatewayId == filter.SiteOrGatewayId);
                }
                if (filter?.Certificate != null) {
                    // If cert thumbprint provided, include it in search
                    query = query.Where(x => x.Thumbprint == filter.Certificate);
                }
                if (filter?.SecurityMode != null) {
                    // If SecurityMode provided, include it in search
                    query = query.Where(x => x.SecurityMode == filter.SecurityMode);
                }
                if (filter?.SecurityPolicy != null) {
                    // If SecurityPolicy uri provided, include it in search
                    query = query.Where(x => x.SecurityPolicy == filter.SecurityPolicy);
                }
                if (filter?.EndpointState != null && filter?.Connected != false &&
                    filter?.Activated != false) {
                    query = query.Where(x => x.State == filter.EndpointState);

                    // Force query for activated and connected
                    filter.Connected = true;
                    filter.Activated = true;
                }
                if (filter?.Activated != null) {
                    // If flag provided, include it in search
                    if (filter.Activated.Value) {
                        query = query.Where(x => x.Activated == true);
                    }
                    else {
                        query = query.Where(x => x.Activated != true);
                    }
                }
                if (filter?.Connected != null) {
                    // If flag provided, include it in search
                    if (filter.Connected.Value) {
                        query = query.Where(x => x.Connected == true);
                    }
                    else {
                        query = query.Where(x => x.Connected != true);
                    }
                }
            }
            query = query.Where(x => x.DeviceType == IdentityType.Endpoint);
            return query.GetResults();
        }

        private readonly IItemContainer _documents;
        private readonly IJsonSerializer _serializer;
    }
}