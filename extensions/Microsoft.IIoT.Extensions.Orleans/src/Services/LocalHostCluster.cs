// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Services {
    using global::Orleans;
    using global::Orleans.Hosting;

    /// <summary>
    /// Default local cluster startup
    /// </summary>
    public class LocalHostCluster : OrleansStartup {

        /// <summary>
        /// Configure client
        /// </summary>
        /// <param name="builder"></param>
        protected override void Configure(IClientBuilder builder) {
            builder
                .UseLocalhostClustering();
        }

        /// <summary>
        /// Configure silo
        /// </summary>
        /// <param name="builder"></param>
        protected override void Configure(ISiloBuilder builder) {
            builder
                .UseLocalhostClustering();
        }
    }
}