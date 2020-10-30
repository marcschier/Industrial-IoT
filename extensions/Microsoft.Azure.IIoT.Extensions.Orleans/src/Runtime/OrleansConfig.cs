// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Orleans.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Orleans configuration
    /// </summary>
    public class OrleansConfig : ConfigBase, IOrleansConfig, 
        IOrleansBusConfig {

        private const string kOrleansClusterId = "Orleans:ClusterId";
        private const string kOrleansServiceId = "Orleans:ServiceId";
        private const string kOrleansPrefix = "Orleans:Prefix";

        /// <inheritdoc/>
        public string ClusterId => GetStringOrDefault(kOrleansClusterId,
            () => GetStringOrDefault("PCS_ORLEANS_CLUSTERID",
                () => null));
        /// <inheritdoc/>
        public string ServiceId => GetStringOrDefault(kOrleansServiceId,
            () => GetStringOrDefault("PCS_ORLEANS_SERVICEID",
                () => null));

        /// <inheritdoc/>
        public string Prefix => GetStringOrDefault(kOrleansPrefix,
                () => null);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public OrleansConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
