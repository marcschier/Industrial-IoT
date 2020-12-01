// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Storage {
    using Microsoft.IIoT.Platform.Identity.Models;
    using Microsoft.IIoT.Storage;
    using Microsoft.IIoT.Exceptions;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.Extensions.Logging;
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
        public ResourceDatabase(ICollectionFactory factory, ILogger logger) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            _documents = factory.OpenAsync("resources").Result;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Resource resource, CancellationToken ct) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }
            var document = resource.ToDocumentModel();
            await _documents.AddAsync(document, ct: ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Resource resource, string etag, CancellationToken ct) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }
            var document = await _documents.GetAsync<ResourceDocumentModel>(
                resource.Name, ct: ct).ConfigureAwait(false);
            if (etag != null && document.Etag != etag) {
                throw new ResourceOutOfDateException();
            }
            await _documents.ReplaceAsync(document, resource.ToDocumentModel(), ct: ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<(Resource, string)> GetAsync(string resourceName, CancellationToken ct) {
            if (string.IsNullOrEmpty(resourceName)) {
                throw new ArgumentNullException(nameof(resourceName));
            }
            var document = await _documents.GetAsync<ResourceDocumentModel>(resourceName, ct: ct).ConfigureAwait(false);
            return (document.Value.ToServiceModel(), document.Etag);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string resourceName, string etag,
            CancellationToken ct) {
            await _documents.DeleteAsync<ResourceDocumentModel>(resourceName, null, etag, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(
            IEnumerable<string> scopeNames) {
            var results = CreateNameQuery(_documents.CreateQuery<ResourceDocumentModel>(),
                scopeNames, nameof(ApiScope));

            var apiScopes = new List<ApiScope>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync().ConfigureAwait(false);
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiScopes.AddRange(resources.OfType<ApiScope>());
            }
            return apiScopes;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames) {
            var results = CreateNameQuery(_documents.CreateQuery<ResourceDocumentModel>(),
                scopeNames, nameof(IdentityResource));

            var identityResources = new List<IdentityResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync().ConfigureAwait(false);
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                identityResources.AddRange(resources.OfType<IdentityResource>());
            }
            return identityResources;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames) {
            var results = CreateScopeQuery(_documents.CreateQuery<ResourceDocumentModel>(),
                scopeNames, nameof(ApiResource));
            var apiResources = new List<ApiResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync().ConfigureAwait(false);
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiResources.AddRange(resources.OfType<ApiResource>());
            }
            return apiResources;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(
            IEnumerable<string> apiResourceNames) {
            var results = CreateNameQuery(_documents.CreateQuery<ResourceDocumentModel>(),
                apiResourceNames, nameof(ApiResource));

            var apiResources = new List<ApiResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync().ConfigureAwait(false);
                var resources = documents.Select(d => d.Value.ToServiceModel()).ToList();
                apiResources.AddRange(resources.OfType<ApiResource>());
            }
            return apiResources;
        }

        /// <inheritdoc/>
        public async Task<ApiResource> FindApiResourceAsync(string name) {
            var resource = await _documents.FindAsync<ResourceDocumentModel>(name).ConfigureAwait(false);
            if (resource?.Value == null) {
                return null;
            }
            return resource.Value.ToServiceModel() as ApiResource;
        }

        /// <inheritdoc/>
        public async Task<Resources> GetAllResourcesAsync() {
            var results = _documents.CreateQuery<ResourceDocumentModel>().GetResults();
            var apiResources = new List<ApiResource>();
            var apiScopes = new List<ApiScope>();
            var identityResources = new List<IdentityResource>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync().ConfigureAwait(false);
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
        /// <param name="query"></param>
        /// <param name="scopeNames"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<ResourceDocumentModel>> CreateScopeQuery(
            IQuery<ResourceDocumentModel> query, IEnumerable<string> scopeNames, string resourceType) {
            var normalizedNames = scopeNames
                .Select(s => s.ToLowerInvariant()).ToArray();
            return query
                .Where(x => x.ResourceType == resourceType)
                .Where(x => x.Scopes != null)
                .Where(x => x.Scopes.Any(s => scopeNames.Contains(s)))
                .GetResults();
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="names"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private static IResultFeed<IDocumentInfo<ResourceDocumentModel>> CreateNameQuery(
            IQuery<ResourceDocumentModel> query, IEnumerable<string> names, string resourceType) {
            var normalizedNames = names
                .Select(s => s.ToLowerInvariant()).ToArray();
            return query
                .Where(x => x.ResourceType == resourceType)
                .Where(x => normalizedNames.Contains(x.Name))
                .GetResults();
        }

        private readonly ILogger _logger;
        private readonly IDocumentCollection _documents;
    }
}