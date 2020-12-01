// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Clients {
    using Microsoft.IIoT.Storage;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using CouchDB.Driver;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    public class CouchDbClient : IDatabaseServer, IHealthCheck {

        /// <summary>
        /// Creates server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public CouchDbClient(IOptions<CouchDbOptions> config, ILogger logger) {
            _options = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (_options.Value.HostName == null) {
                throw new ArgumentNullException("Host name missing", nameof(_options));
            }
        }

        /// <inheritdoc/>
        public Task<IDatabase> OpenAsync(string databaseId, DatabaseOptions options) {
            var client = new CouchClient("http://" + _options.Value.HostName + ":5984",
                builder => {
                    builder
                        .EnsureDatabaseExists()
                        .UseBasicAuthentication(_options.Value.UserName, _options.Value.Key)
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

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken ct) {
            await using (var client = new CouchClient("http://" + _options.Value.HostName + ":5984",
                builder => builder
                    .EnsureDatabaseExists()
                    .UseBasicAuthentication(_options.Value.UserName, _options.Value.Key)
                    .IgnoreCertificateValidation()
            )) {
                try {
                    // Try get last item
                    var up = await client.IsUpAsync(ct).ConfigureAwait(false);
                    return up ? HealthCheckResult.Healthy() :
                        HealthCheckResult.Degraded();
                }
                catch (Exception ex) {
                    return HealthCheckResult.Unhealthy("Not up", ex);
                }
            }
        }

        private readonly IOptions<CouchDbOptions> _options;
        private readonly ILogger _logger;
    }
}
