// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.CouchDb.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// CouchDb configuration
    /// </summary>
    public class CouchDbConfig : ConfigBase, ICouchDbConfig {

        private const string kCouchDbHostName = "CouchDb:HostName";
        private const string kCouchDbUserName = "CouchDb:UserName";
        private const string kCouchDbKey = "CouchDb:Key";

        /// <inheritdoc/>
        public string HostName => GetStringOrDefault(kCouchDbHostName,
            () => GetStringOrDefault(PcsVariable.PCS_COUCHDB_HOSTNAME,
                () => "localhost"));
        /// <inheritdoc/>
        public string UserName => GetStringOrDefault(kCouchDbUserName,
            () => GetStringOrDefault(PcsVariable.PCS_COUCHDB_USERNAME,
                () => "admin"));
        /// <inheritdoc/>
        public string Key => GetStringOrDefault(kCouchDbKey,
            () => GetStringOrDefault(PcsVariable.PCS_COUCHDB_KEY,
                () => "couchdb"));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CouchDbConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
