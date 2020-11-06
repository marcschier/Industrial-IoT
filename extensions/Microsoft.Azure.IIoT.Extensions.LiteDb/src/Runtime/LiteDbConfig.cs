// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.LiteDb.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// LiteDb configuration
    /// </summary>
    internal sealed class LiteDbConfig : PostConfigureOptionBase<LiteDbOptions> {

        /// <inheritdoc/>
        public LiteDbConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, LiteDbOptions options) {
            if (string.IsNullOrEmpty(options.DbConnectionString)) {
                options.DbConnectionString = 
                    GetStringOrDefault(PcsVariable.PCS_COSMOSDB_CONNSTRING,
                    GetStringOrDefault("_DB_CS"));
            }
        }
    }
}
