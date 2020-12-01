// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Clients {
    using Microsoft.IIoT.Storage;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Generic;
    using CouchDB.Driver;

    /// <summary>
    /// Lite database
    /// </summary>
    internal sealed class CouchDbDatabase : IDatabase {

        /// <summary>
        /// Creates database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="logger"></param>
        internal CouchDbDatabase(CouchClient db, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <inheritdoc/>
        public async Task<IDocumentCollection> OpenContainerAsync(string id,
            ContainerOptions options) {
            if (string.IsNullOrEmpty(id)) {
                id = "default";
            }
            var db = await _client.GetOrCreateDatabaseAsync<CouchDbDocument>(id).ConfigureAwait(false);
            return new CouchDbCollection(id, db, _logger);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListContainersAsync(CancellationToken ct) {
            return await _client.GetDatabasesNamesAsync(ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task DeleteContainerAsync(string id) {
            if (string.IsNullOrEmpty(id)) {
                id = "default";
            }
            return _client.DeleteDatabaseAsync(id);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _client.DisposeAsync().AsTask().Wait();
        }

        private readonly CouchClient _client;
        private readonly ILogger _logger;
    }
}
