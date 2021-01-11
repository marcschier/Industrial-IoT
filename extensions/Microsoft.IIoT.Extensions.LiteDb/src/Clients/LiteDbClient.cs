// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.LiteDb.Clients {
    using Microsoft.IIoT.Extensions.Storage;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using LiteDB;
    using System;
    using System.Threading.Tasks;
    using System.IO;
    using System.Globalization;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public class LiteDbClient : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public LiteDbClient(IOptionsSnapshot<LiteDbOptions> options, ILogger logger) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrEmpty(_options.Value.DbConnectionString)) {
                throw new ArgumentException("Missing connection string", nameof(options));
            }
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            var cs = new ConnectionString(_options.Value.DbConnectionString);
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            cs.Filename = (cs.Filename == null || cs.Filename.Trim(':') != cs.Filename ?
                databaseId : Path.Combine(
                    Path.GetFullPath(cs.Filename), databaseId)) + ".db";
            var client = new LiteDatabase(cs, DocumentSerializer.Mapper) {
                UtcDate = true
            };
            if (client.Collation.SortOptions != CompareOptions.Ordinal) {
                client.Rebuild(new LiteDB.Engine.RebuildOptions {
                    Collation = new Collation(9, CompareOptions.Ordinal)
                });
            }
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly IOptionsSnapshot<LiteDbOptions> _options;
        private readonly ILogger _logger;
    }
}
