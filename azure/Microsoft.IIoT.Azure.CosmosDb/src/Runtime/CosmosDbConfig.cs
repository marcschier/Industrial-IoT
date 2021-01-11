// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.CosmosDb.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// CosmosDb configuration
    /// </summary>
    internal sealed class CosmosDbConfig : PostConfigureOptionBase<CosmosDbOptions> {

        /// <inheritdoc/>
        public CosmosDbConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, CosmosDbOptions options) {
            if (string.IsNullOrEmpty(options.ConnectionString)) {
                options.ConnectionString =
                    GetStringOrDefault(PcsVariable.PCS_COSMOSDB_CONNSTRING,
                    GetStringOrDefault("PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING",
                    GetStringOrDefault("PCS_TELEMETRY_DOCUMENTDB_CONNSTRING",
                    GetStringOrDefault("_DB_CS"))));
            }
            if (options.ThroughputUnits == null) {
                options.ThroughputUnits = GetIntOrDefault("PCS_COSMOSDB_THROUGHPUT", 400);
            }
        }
    }
}
