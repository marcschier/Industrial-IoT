// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.Cosmos;
    using Serilog;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public sealed class CosmosDbServiceClient : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public CosmosDbServiceClient(ICosmosDbConfig config, IJsonSerializer serializer,
            ILogger logger) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            if (string.IsNullOrEmpty(_config.DbConnectionString)) {
                throw new ArgumentException("Connection string missing", nameof(config));
            }
        }

        /// <inheritdoc/>
        public async Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            if (string.IsNullOrEmpty(databaseId)) {
                databaseId = "default";
            }
            var cs = ConnectionString.Parse(_config.DbConnectionString);
#pragma warning disable CA2000 // Dispose objects before losing scope
            var client = new CosmosClient(cs.Endpoint, cs.SharedAccessKey,
                new CosmosClientOptions {
                    ConsistencyLevel = options?.Consistency.ToConsistencyLevel()
                });
#pragma warning restore CA2000 // Dispose objects before losing scope
            var response = await client.CreateDatabaseIfNotExistsAsync(databaseId,
                _config.ThroughputUnits).ConfigureAwait(false);
            return new DocumentDatabase(response.Database, _serializer, _logger);
        }

        private readonly ICosmosDbConfig _config;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
    }
}
