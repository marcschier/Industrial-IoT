// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.Default {
    using Microsoft.Azure.IIoT.Storage.LiteDb.Clients;
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.IO;
    using LiteDB;

    /// <summary>
    /// Provides in memory storage with litedb engine.
    /// </summary>
    public class MemoryDatabase : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="logger"></param>
        public MemoryDatabase(ILogger logger = null) {
            _logger = logger ?? Log.Logger;
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            databaseId = databaseId.Replace('-', '_').ToLowerInvariant();
            var client = _clients.GetOrAdd(
                databaseId, id => new LiteDatabase(new MemoryStream(), DocumentSerializer.Mapper));
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly ConcurrentDictionary<string, LiteDatabase> _clients =
            new ConcurrentDictionary<string, LiteDatabase>();
        private readonly ILogger _logger;
    }
}
