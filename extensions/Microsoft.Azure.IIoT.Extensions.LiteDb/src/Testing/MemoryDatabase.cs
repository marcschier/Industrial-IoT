// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Services {
    using Microsoft.Azure.IIoT.Extensions.LiteDb.Clients;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.IO;
    using LiteDB;
    using System.Globalization;

    /// <summary>
    /// Provides in memory storage with litedb engine.
    /// </summary>
    public class MemoryDatabase : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="logger"></param>
        public MemoryDatabase(ILogger logger = null) {
            _logger = logger ?? Log.Console();
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            databaseId = databaseId.Replace('-', '_').ToLowerInvariant();
            var client = _clients.GetOrAdd(databaseId, id => Open());
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        /// <summary>
        /// Helper to create client
        /// </summary>
        /// <returns></returns>
        private static LiteDatabase Open() {
            var client = new LiteDatabase(new MemoryStream(), DocumentSerializer.Mapper) {
                UtcDate = true
            };
            client.Rebuild(new LiteDB.Engine.RebuildOptions {
                Collation = new Collation(9, CompareOptions.Ordinal)
            });
            return client;
        }

        private readonly ConcurrentDictionary<string, LiteDatabase> _clients =
            new ConcurrentDictionary<string, LiteDatabase>();
        private readonly ILogger _logger;
    }
}
