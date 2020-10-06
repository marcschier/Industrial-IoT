// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Storage.Default {
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
    /// Database application repository
    /// </summary>
    public class ApplicationDatabase : IApplicationRepository {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="config"></param>
        public ApplicationDatabase(IDatabaseServer databaseServer,
            IItemContainerConfig config) {
            if (databaseServer is null) {
                throw new ArgumentNullException(nameof(databaseServer));
            }
            if (config is null) {
                throw new ArgumentNullException(nameof(config));
            }
            var dbs = databaseServer.OpenAsync(config.DatabaseName).Result;
            _documents = dbs.OpenContainerAsync(config.ContainerName ?? "twin").Result;
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoModel> AddAsync(ApplicationInfoModel application,
            CancellationToken ct) {
            if (application == null) {
                throw new ArgumentNullException(nameof(application));
            }
            var presetId = application.ApplicationId;
            while (true) {
                if (!string.IsNullOrEmpty(application.ApplicationId)) {
                    var document = await _documents.FindAsync<ApplicationDocument>(
                        application.ApplicationId, ct: ct).ConfigureAwait(false);
                    if (document != null) {
                        throw new ResourceConflictException(
                            $"Writer Group {application.ApplicationId} already exists.");
                    }
                }
                else {
                    application.ApplicationId = Guid.NewGuid().ToString();
                }
                try {
                    var result = await _documents.AddAsync(application.ToDocumentModel(), 
                        ct: ct).ConfigureAwait(false);
                    return result.Value.ToServiceModel(result.Etag);
                }
                catch (ResourceConflictException) {
                    // Try again - reset to preset id or null if none was asked for
                    application.ApplicationId = presetId;
                    continue;
                }
                catch {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoModel> AddOrUpdateAsync(string applicationId,
            Func<ApplicationInfoModel, Task<ApplicationInfoModel>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            while (true) {
                var document = await _documents.FindAsync<ApplicationDocument>(
                    applicationId, ct: ct).ConfigureAwait(false);
                var updateOrAdd = document?.Value.ToServiceModel(document.Etag);
                var application = await predicate(updateOrAdd).ConfigureAwait(false);
                if (application == null) {
                    return updateOrAdd;
                }
                application.ApplicationId = applicationId;
                var updated = application.ToDocumentModel();
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
        public async Task<ApplicationInfoModel> UpdateAsync(string applicationId,
            Func<ApplicationInfoModel, Task<bool>> predicate, CancellationToken ct) {

            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            while (true) {
                var document = await _documents.FindAsync<ApplicationDocument>(
                    applicationId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    throw new ResourceNotFoundException("Writer application not found");
                }
                var application = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(application).ConfigureAwait(false)) {
                    return application;
                }
                application.ApplicationId = applicationId;
                var updated = application.ToDocumentModel();
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
        public async Task<ApplicationInfoModel> FindAsync(string applicationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            var document = await _documents.FindAsync<ApplicationDocument>(
                applicationId, ct: ct).ConfigureAwait(false);
            if (document == null) {
                return null;
            }
            return document.Value.ToServiceModel(document.Etag);
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoListModel> QueryAsync(ApplicationRegistrationQueryModel query,
            string continuationToken, int? maxResults, CancellationToken ct) {
            var results = continuationToken != null ?
                _documents.ContinueQuery<ApplicationDocument>(continuationToken, maxResults) :
                CreateQuery(_documents.CreateQuery<ApplicationDocument>(maxResults), query);
            if (!results.HasMore()) {
                return new ApplicationInfoListModel {
                    Items = new List<ApplicationInfoModel>()
                };
            }
            var documents = await results.ReadAsync(ct).ConfigureAwait(false);
            return new ApplicationInfoListModel {
                ContinuationToken = results.ContinuationToken,
                Items = documents.Select(r => r.Value.ToServiceModel(r.Etag)).ToList()
            };
        }

        /// <inheritdoc/>
        public async Task<ApplicationInfoModel> DeleteAsync(string applicationId,
            Func<ApplicationInfoModel, Task<bool>> predicate, CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            while (true) {
                var document = await _documents.FindAsync<ApplicationDocument>(
                    applicationId, ct: ct).ConfigureAwait(false);
                if (document == null) {
                    return null;
                }
                var application = document.Value.ToServiceModel(document.Etag);
                if (!await predicate(application).ConfigureAwait(false)) {
                    return null;
                }
                try {
                    await _documents.DeleteAsync(document, ct: ct).ConfigureAwait(false);
                }
                catch (ResourceOutOfDateException) {
                    continue;
                }
                return application;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string applicationId, string generationId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(applicationId)) {
                throw new ArgumentNullException(nameof(applicationId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _documents.DeleteAsync<ApplicationDocument>(
                applicationId, null, generationId, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<ApplicationDocument>> CreateQuery(
            IQuery<ApplicationDocument> query, ApplicationRegistrationQueryModel filter) {

            if (filter != null) {
                if (!(filter?.IncludeNotSeenSince ?? false)) {
                    // Scope to non deleted applications
                    query = query.Where(x => x.NotSeenSince == null);
                }
                if (filter?.Locale != null) {
                    if (filter?.ApplicationName != null) {
                        // If application name provided, include it in search
                        query = query.Where(x =>
                            x.LocalizedNames[filter.Locale] == filter.ApplicationName);
                    }
                    else {
                        // Just search for locale
                        query = query.Where(x => x.LocalizedNames.ContainsKey(filter.Locale));
                    }
                }
                else if (filter?.ApplicationName != null) {
                    // If application name provided, search for default name
                    query = query.Where(x => x.ApplicationName == filter.ApplicationName);
                }
                if (filter?.DiscovererId != null) {
                    // If discoverer provided, include it in search
                    query = query.Where(x => x.DiscovererId == filter.DiscovererId);
                }
                if (filter?.ProductUri != null) {
                    // If product uri provided, include it in search
                    query = query.Where(x => x.ProductUri == filter.ProductUri);
                }
                if (filter?.GatewayServerUri != null) {
                    // If gateway uri provided, include it in search
                    query = query.Where(x => x.GatewayServerUri == filter.GatewayServerUri);
                }
                if (filter?.DiscoveryProfileUri != null) {
                    // If discovery profile uri provided, include it in search
                    query = query.Where(x => x.DiscoveryProfileUri == filter.DiscoveryProfileUri);
                }
                if (filter?.ApplicationUri != null) {
                    // If ApplicationUri provided, include it in search
                    query = query.Where(x => x.ApplicationUriUC ==
                        filter.ApplicationUri.ToLowerInvariant());
                }
                if (filter?.ApplicationType != null) {
                    // If searching for clients include it in search
                    query = query.Where(x => filter.ApplicationType.Value ==
                        (x.ApplicationType & filter.ApplicationType.Value));
                }
                if (filter?.Capability != null) {
                    // If Capabilities provided, filter results
                    query = query.Where(x => x.Capabilities.ContainsKey(filter.Capability));
                }
            }
            query = query.Where(x => x.ClassType == IdentityType.Application);
            return query.GetResults();
        }

        private readonly IItemContainer _documents;
    }
}