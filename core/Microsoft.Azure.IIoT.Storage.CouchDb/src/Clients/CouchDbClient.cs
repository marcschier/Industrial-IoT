﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Clients {
    using Microsoft.Azure.IIoT.Storage;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using CouchDB.Driver;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public class CouchDbClient : IDatabaseServer {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public CouchDbClient(ICouchDbConfig config, ILogger logger) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (_config.HostName == null) {
                throw new ArgumentNullException(nameof(_config.HostName));
            }
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            var client = new CouchClient("http://" + _config.HostName + ":5984", builder => {
                builder
                    .EnsureDatabaseExists()
                    .UseBasicAuthentication(_config.UserName, _config.Key)
                    .IgnoreCertificateValidation()
                    .ConfigureFlurlClient(client => {
                        // client.HttpClientFactory =
                    })
                    // ...
                    ;
            });
            var db = new CouchDbDatabase(client, _logger);
            return Task.FromResult<IDatabase>(db);
        }

        private readonly ICouchDbConfig _config;
        private readonly ILogger _logger;
    }
}
