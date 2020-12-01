// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.CouchDb.Runtime {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// CouchDb configuration
    /// </summary>
    internal sealed class CouchDbConfig : PostConfigureOptionBase<CouchDbOptions> {

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CouchDbConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, CouchDbOptions options) {
            if (string.IsNullOrEmpty(options.HostName)) {
                options.HostName =
                    GetStringOrDefault(PcsVariable.PCS_COUCHDB_HOSTNAME, "localhost");
            }
            if (string.IsNullOrEmpty(options.UserName)) {
                options.UserName =
                    GetStringOrDefault(PcsVariable.PCS_COUCHDB_USERNAME, "admin");
            }
            if (string.IsNullOrEmpty(options.Key)) {
                options.Key =
                    GetStringOrDefault(PcsVariable.PCS_COUCHDB_KEY, "couchdb");
            }
        }
    }
}
