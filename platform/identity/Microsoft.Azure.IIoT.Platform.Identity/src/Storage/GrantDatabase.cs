// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Storage {
    using Microsoft.Azure.IIoT.Platform.Identity.Models;
    using Microsoft.Azure.IIoT.Storage;
    using IdentityServer4.Stores;
    using IdentityServer4.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Grant store
    /// </summary>
    public class GrantDatabase : IPersistedGrantStore {

        /// <summary>
        /// Create grant storage
        /// </summary>
        /// <param name="factory"></param>
        public GrantDatabase(IItemContainerFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            _documents = factory.OpenAsync("grants").Result.AsDocuments();
        }

        /// <inheritdoc/>
        public async Task<PersistedGrant> GetAsync(string key) {
            var grant = await _documents.FindAsync<GrantDocumentModel>(key);
            if (grant?.Value == null) {
                return null;
            }
            return grant.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter) {
            if (filter == null) {
                throw new ArgumentNullException(nameof(filter));
            }
            var results = CreateQuery(_documents.CreateQuery<GrantDocumentModel>(), filter);
            var grants = new List<PersistedGrant>();
            while (results.HasMore()) {
                var documents = await results.ReadAsync();
                grants.AddRange(
                    documents.Select(d => d.Value.ToServiceModel()));
            }
            return grants;
        }

        /// <inheritdoc/>
        public async Task StoreAsync(PersistedGrant token) {
            var document = token.ToDocumentModel();
            await _documents.UpsertAsync(document);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key) {
            await _documents.DeleteAsync<GrantDocumentModel>(key);
        }

        /// <inheritdoc/>
        public async Task RemoveAllAsync(PersistedGrantFilter filter) {
            if (filter == null) {
                throw new ArgumentNullException(nameof(filter));
            }
            var results = CreateQuery(_documents.CreateQuery<GrantDocumentModel>(), 
                filter);
            await results.ForEachAsync(d =>
                _documents.DeleteAsync<GrantDocumentModel>(d.Id));
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private IResultFeed<IDocumentInfo<GrantDocumentModel>> CreateQuery(
            IQuery<GrantDocumentModel> query, PersistedGrantFilter filter) {

            if (filter.ClientId != null) {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (filter.SubjectId != null) {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (filter.SessionId != null) {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (filter.Type != null) {
                query = query.Where(x => x.Type == filter.Type);
            }
            var now = DateTime.UtcNow;
            query = query.Where(x => x.Expiration < now);
            return query.GetResults();
        }

        private readonly IDocuments _documents;
    }
}