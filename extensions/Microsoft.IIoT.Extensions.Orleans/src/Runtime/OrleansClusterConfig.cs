// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using global::Orleans.Configuration;

    /// <summary>
    /// Orleans configuration
    /// </summary>
    internal sealed class OrleansClusterConfig : ConfigureOptionBase<ClusterOptions> {

        /// <inheritdoc/>
        public OrleansClusterConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, ClusterOptions options) {
            if (string.IsNullOrEmpty(options.ClusterId)) {
                options.ClusterId = GetStringOrDefault("PCS_ORLEANS_CLUSTERID");
            }

            if (string.IsNullOrEmpty(options.ServiceId)) {
                options.ServiceId = GetStringOrDefault("PCS_ORLEANS_SERVICEID");
            }
        }
    }
}
