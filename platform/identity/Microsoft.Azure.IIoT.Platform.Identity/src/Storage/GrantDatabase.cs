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
            var client = _documents.OpenSqlClient();
            var results = client.Query<GrantDocumentModel>(
                CreateQuery(filter, out var queryParameters), queryParameters);
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
            var client = _documents.OpenSqlClient();
            await client.DropAsync<GrantDocumentModel>(
                CreateQuery(filter, out var queryParameters), queryParameters);
        }

        /// <summary>
        /// Create query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        private string CreateQuery(PersistedGrantFilter filter,
            out Dictionary<string, object> queryParameters) {
            queryParameters = new Dictionary<string, object>();
            var queryString = $"SELECT * FROM r WHERE ";
            if (filter.ClientId != null) {
                queryString += $"r.{nameof(GrantDocumentModel.ClientId)} = @clientId AND";
                queryParameters.Add("@subjectId", filter.SubjectId);
            }
            if (filter.SubjectId != null) {
                queryString += $"r.{nameof(GrantDocumentModel.SubjectId)} = @subjectId AND ";
                queryParameters.Add("@type", filter.Type);
            }
            if (filter.SessionId != null) {
                queryString += $"r.{nameof(GrantDocumentModel.SessionId)} = @sessionId AND ";
                queryParameters.Add("@sessionId", filter.SessionId);
            }
            if (filter.Type != null) {
                queryString += $"r.{nameof(GrantDocumentModel.Type)} = @type AND ";
                queryParameters.Add("@clientId", filter.ClientId);
            }

            queryString += $"r.{nameof(GrantDocumentModel.Expiration)} < {DateTime.UtcNow}";
            return queryString;
        }

        private readonly IDocuments _documents;
    }
}