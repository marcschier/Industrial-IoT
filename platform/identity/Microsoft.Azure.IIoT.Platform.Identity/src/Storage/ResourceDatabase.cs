﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Storage {
    using Microsoft.Azure.IIoT.Platform.Identity.Models;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Exceptions;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Resource store
    /// </summary>
    public class ResourceDatabase : IResourceStore, IResourceRepository {

        /// <summary>
        /// Create resource store
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        public ResourceDatabase(IItemContainerFactory factory, ILogger logger) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            _documents = factory.OpenAsync("resources").Result.AsDocuments();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Resource resource, CancellationToken ct) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }
            var document = resource.ToDocumentModel();
            await _documents.AddAsync(document, ct);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Resource resource, string etag, CancellationToken ct) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }
            var document = await _documents.GetAsync<ResourceDocumentModel>(
                resource.Name, ct);
            if (etag != null && document.Etag != etag) {
                throw new ResourceOutOfDateException();
            }
            await _documents.ReplaceAsync(document, resource.ToDocumentModel(), ct);
        }

        /// <inheritdoc/>
        public async Task<(Resource, string)> GetAsync(string resourceName, CancellationToken ct) {
            if (string.IsNullOrEmpty(resourceName)) {
                throw new ArgumentNullException(nameof(resourceName));
            }
            var document = await _documents.GetAsync<ResourceDocumentModel>(resourceName, ct);
            return (document.Value.ToServiceModel(), document.Etag);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string resourceName, string etag,
            CancellationToken ct) {
            await _documents.DeleteAsync<ResourceDocumentModel>(resourceName, ct, null, etag);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(
            IEnumerable<string> scopeNames) {
            var resource = _documents.OpenSqlClient();
            var results = resource.Query<ResourceDocumentModel>(
                CreateNameQuery(out var queryParameters, scopeNames, nameof(ApiScope)),
                    queryParameters);

            var apiScopes = new List<ApiScope>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiScopes.AddRange(resources.OfType<ApiScope>());
            }
            return apiScopes;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames) {
            var resource = _documents.OpenSqlClient();
            var results = resource.Query<ResourceDocumentModel>(
                CreateNameQuery(out var queryParameters, scopeNames, nameof(IdentityResource)),
                    queryParameters);

            var identityResources = new List<IdentityResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                identityResources.AddRange(resources.OfType<IdentityResource>());
            }
            return identityResources;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames) {
            var resource = _documents.OpenSqlClient();
            var results = resource.Query<ResourceDocumentModel>(
                CreateScopeQuery(out var queryParameters, scopeNames, nameof(ApiResource)),
                    queryParameters);

            var apiResources = new List<ApiResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiResources.AddRange(resources.OfType<ApiResource>());
            }
            return apiResources;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(
            IEnumerable<string> apiResourceNames) {
            var resource = _documents.OpenSqlClient();
            var results = resource.Query<ResourceDocumentModel>(
                CreateNameQuery(out var queryParameters, apiResourceNames, nameof(ApiResource)),
                    queryParameters);

            var apiResources = new List<ApiResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiResources.AddRange(resources.OfType<ApiResource>());
            }
            return apiResources;
        }

        /// <inheritdoc/>
        public async Task<ApiResource> FindApiResourceAsync(string name) {
            var resource = await _documents.FindAsync<ResourceDocumentModel>(name);
            if (resource?.Value == null) {
                return null;
            }
            return resource.Value.ToServiceModel() as ApiResource;
        }

        /// <inheritdoc/>
        public async Task<Resources> GetAllResourcesAsync() {
            var resource = _documents.OpenSqlClient();
            var results = resource.Query<ResourceDocumentModel>("SELECT * FROM r");
            var apiResources = new List<ApiResource>();
            var apiScopes = new List<ApiScope>();
            var identityResources = new List<IdentityResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();

                apiScopes.AddRange(resources.OfType<ApiScope>());
                apiResources.AddRange(resources.OfType<ApiResource>());
                identityResources.AddRange(resources.OfType<IdentityResource>());
            }
            return new Resources {
                IdentityResources = identityResources,
                ApiResources = apiResources,
                ApiScopes = apiScopes
            };
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <param name="scopeNames"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private static string CreateScopeQuery(out Dictionary<string, object> queryParameters,
            IEnumerable<string> scopeNames, string resourceType) {
            queryParameters = new Dictionary<string, object> {
                { "@scopes", scopeNames
                      .Select(s => s.ToLowerInvariant()).ToList() }
            };
            var queryString = $"SELECT r FROM r JOIN " +
$"(SELECT VALUE scope FROM scope IN r.{nameof(ResourceDocumentModel.Scopes)} WHERE scope IN (@scopes)) " +
$"WHERE r.{nameof(ResourceDocumentModel.ResourceType)} = '{resourceType}'";
            return queryString;
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <param name="names"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private static string CreateNameQuery(out Dictionary<string, object> queryParameters,
            IEnumerable<string> names, string resourceType) {
            queryParameters = new Dictionary<string, object> {
                { "@names", names
                      .Select(s => s.ToLowerInvariant()).ToList() }
            };
            var queryString = $"SELECT * FROM r WHERE ";
            queryString +=
$"r.{nameof(ResourceDocumentModel.Name)} IN (@names)' AND ";
            queryString +=
$"r.{nameof(ResourceDocumentModel.ResourceType)} = '{resourceType}'";
            return queryString;
        }

        private readonly ILogger _logger;
        private readonly IDocuments _documents;
    }
}