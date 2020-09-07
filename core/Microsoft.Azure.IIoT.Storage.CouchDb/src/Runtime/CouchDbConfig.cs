// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// CouchDb configuration
    /// </summary>
    public class CouchDbConfig : ConfigBase, ICouchDbConfig {

        private const string kCouchDbConnectionString = "CouchDb:ConnectionString";

        /// <inheritdoc/>
        public string HostName => GetStringOrDefault(kCouchDbConnectionString,
            () => GetStringOrDefault(PcsVariable.PCS_COSMOSDB_CONNSTRING,
                () => GetStringOrDefault("_DB_CS",
                    () => null)));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CouchDbConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
