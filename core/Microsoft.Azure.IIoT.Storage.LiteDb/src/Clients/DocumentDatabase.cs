// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Net;
    using LiteDB;

    /// <summary>
    /// Provides document db database interface.
    /// </summary>
    internal sealed class DocumentDatabase : IDatabase {

        /// <summary>
        /// Database id
        /// </summary>
        internal string DatabaseId { get; }

        /// <summary>
        /// Client
        /// </summary>
        internal LiteDatabase Client { get; }

        /// <summary>
        /// Creates database
        /// </summary>
        /// <param name="client"></param>
        /// <param name="databaseId"></param>
        /// <param name="logger"></param>
        /// <param name="jsonConfig"></param>
        internal DocumentDatabase(LiteDatabase client, string databaseId,
            ILogger logger, IJsonSerializerSettingsProvider jsonConfig = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            _jsonConfig = jsonConfig;
            _collections = new ConcurrentDictionary<string, DocumentCollection>();
        }

        /// <inheritdoc/>
        public async Task<IItemContainer> OpenContainerAsync(string id,
            ContainerOptions options) {
            return await OpenOrCreateCollectionAsync(id, options);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListContainersAsync(CancellationToken ct) {
            var continuation = string.Empty;
            var result = new List<string>();
            do {
                var response = await Client.ReadDocumentCollectionFeedAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId),
                    new FeedOptions {
                        RequestContinuation = continuation
                    });
                continuation = response.ResponseContinuation;
                result.AddRange(response.Select(c => c.Id));
            }
            while (!string.IsNullOrEmpty(continuation));
            return result;
        }

        /// <inheritdoc/>
        public async Task DeleteContainerAsync(string id) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            await Client.DeleteDocumentCollectionAsync(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, id));
            _collections.TryRemove(id, out _);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _collections.Clear();
            Client.Dispose();
        }

        /// <summary>
        /// Create or Open collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private async Task<DocumentCollection> OpenOrCreateCollectionAsync(
            string id, ContainerOptions options) {
            if (string.IsNullOrEmpty(id)) {
                id = "default";
            }
            if (!_collections.TryGetValue(id, out var collection)) {
                var coll = await EnsureCollectionExistsAsync(id, options);
                collection = _collections.GetOrAdd(id, k =>
                    new DocumentCollection(this, coll, _logger, _jsonConfig));
            }
            return collection;
        }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, DocumentCollection> _collections;
        private readonly IJsonSerializerSettingsProvider _jsonConfig;
    }
}
