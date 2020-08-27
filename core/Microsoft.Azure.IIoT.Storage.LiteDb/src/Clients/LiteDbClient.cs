// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.IO;
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
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            ILiteDatabase client;
            if (string.IsNullOrEmpty(_config.DbConnectionString)) {
                client = new LiteDatabase(new MemoryStream(), _mapper);
            }
            else {
                client = new LiteDatabase(_config.DbConnectionString, _mapper);
            }
            var db = new DocumentDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly BsonMapper _mapper = new BsonMapper();
        private readonly ILiteDbConfig _config;
        private readonly ILogger _logger;
    }
}
