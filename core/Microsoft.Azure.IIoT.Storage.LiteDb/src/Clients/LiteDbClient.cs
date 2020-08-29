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
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            var client = new LiteDatabase(_config.DbConnectionString, DocumentSerializer.Mapper);
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly ILiteDbConfig _config;
        private readonly ILogger _logger;
    }
}
