// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using LiteDB;
    using System.IO;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public class LiteDbClient : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public LiteDbClient(ILiteDbConfig config, ILogger logger) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (_config.DbConnectionString == null) {
                throw new ArgumentNullException(nameof(_config.DbConnectionString));
            }
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            var cs = new ConnectionString(_config.DbConnectionString);
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            cs.Filename = (cs.Filename == null || cs.Filename.Trim(':') != cs.Filename ? 
                databaseId : Path.Combine(
                    Path.GetFullPath(cs.Filename), databaseId)) + ".db";
            var client = new LiteDatabase(cs, DocumentSerializer.Mapper);
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly ILiteDbConfig _config;
        private readonly ILogger _logger;
    }
}
