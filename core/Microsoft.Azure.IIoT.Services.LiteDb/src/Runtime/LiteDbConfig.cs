// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.LiteDb.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// LiteDb configuration
    /// </summary>
    public class LiteDbConfig : ConfigBase, ILiteDbConfig {

        private const string kLiteDbConnectionString = "LiteDb:ConnectionString";

        /// <inheritdoc/>
        public string DbConnectionString => GetStringOrDefault(kLiteDbConnectionString,
            () => GetStringOrDefault(PcsVariable.PCS_COSMOSDB_CONNSTRING,
                () => GetStringOrDefault("_DB_CS",
                    () => null)));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public LiteDbConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
