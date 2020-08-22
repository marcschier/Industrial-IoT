// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using LiteDB;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public sealed class LiteDbClient : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="jsonConfig"></param>
        public LiteDbClient(ILiteDbConfig config,
            ILogger logger, IJsonSerializerSettingsProvider jsonConfig = null) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonConfig = jsonConfig;
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            if (string.IsNullOrEmpty(_config.DbConnectionString)) {

            }
            var client = new LiteDatabase(_config.DbConnectionString);
            var db = new DocumentDatabase(client, databaseId, _logger, _jsonConfig);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly ILiteDbConfig _config;
        private readonly ILogger _logger;
        private readonly IJsonSerializerSettingsProvider _jsonConfig;
    }
}
