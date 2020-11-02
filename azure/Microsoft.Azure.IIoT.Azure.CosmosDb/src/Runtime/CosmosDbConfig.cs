// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// CosmosDb configuration
    /// </summary>
    internal sealed class CosmosDbConfig : ConfigBase<CosmosDbOptions> {

        /// <inheritdoc/>
        public CosmosDbConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, CosmosDbOptions options) {
            options.DbConnectionString = GetStringOrDefault(PcsVariable.PCS_COSMOSDB_CONNSTRING,
                () => GetStringOrDefault("PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING",
                () => GetStringOrDefault("PCS_TELEMETRY_DOCUMENTDB_CONNSTRING",
                    () => GetStringOrDefault("_DB_CS",
                        () => null))));
            options.ThroughputUnits = GetIntOrDefault("PCS_COSMOSDB_THROUGHPUT", () => 400);
        }
    }
}
