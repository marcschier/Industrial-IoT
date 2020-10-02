// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Storage {
    using Microsoft.Azure.IIoT.Platform.Identity.Models;
    using Microsoft.Azure.IIoT.Storage;
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using IdentityServer4.Services;
    using System.Threading;
    using Microsoft.Azure.IIoT.Exceptions;

    /// <summary>
    /// Client store
    /// </summary>
    public class ClientDatabase : IClientStore, ICorsPolicyService, IClientRepository {

        /// <summary>
        /// Create client store
        /// </summary>
        /// <param name="factory"></param>
        public ClientDatabase(IItemContainerFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }
            _documents = factory.OpenAsync("clients").Result;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Client client, CancellationToken ct) {
            if (client == null) {
                throw new ArgumentNullException(nameof(client));
            }
            var document = client.ToDocumentModel();
            await _documents.AddAsync(document, ct: ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Client client, string etag, CancellationToken ct) {
            if (client == null) {
                throw new ArgumentNullException(nameof(client));
            }
            var document = await _documents.GetAsync<ClientDocumentModel>(
                client.ClientId, ct: ct).ConfigureAwait(false);
            if (etag != null && document.Etag != etag) {
                throw new ResourceOutOfDateException();
            }
            await _documents.ReplaceAsync(document, client.ToDocumentModel(), ct: ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<(Client, string)> GetAsync(string clientId, CancellationToken ct) {
            if (string.IsNullOrEmpty(clientId)) {
                throw new ArgumentNullException(nameof(clientId));
            }
            var document = await _documents.GetAsync<ClientDocumentModel>(clientId, ct: ct).ConfigureAwait(false);
            return (document.Value.ToServiceModel(), document.Etag);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string clientId, string etag,
            CancellationToken ct) {
            await _documents.DeleteAsync<ClientDocumentModel>(clientId, null, etag, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Client> FindClientByIdAsync(string clientId) {
            var document = await _documents.FindAsync<ClientDocumentModel>(clientId).ConfigureAwait(false);
            if (document?.Value == null) {
                return null;
            }
            return document.Value.ToServiceModel();
        }

        /// <inheritdoc/>
        public async Task<bool> IsOriginAllowedAsync(string origin) {
            if (origin == null) {
                throw new ArgumentNullException(nameof(origin));
            }
            origin = origin.ToLowerInvariant();
            var results = await _documents.CreateQuery<ClientDocumentModel>(1)
                .Where(x => x.AllowedCorsOrigins != null)
                .Where(x => x.AllowedCorsOrigins.Contains(origin))
                .CountAsync().ConfigureAwait(false);
            return results != 0;
        }

        private readonly IItemContainer _documents;
    }
}