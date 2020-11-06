// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb {
    using Microsoft.Azure.IIoT.Azure.CosmosDb.Runtime;
    using Microsoft.Azure.IIoT.Azure.CosmosDb.Clients;
    using Microsoft.Azure.IIoT.Storage;
    using Autofac;

    /// <summary>
    /// Injected Cosmos client
    /// </summary>
    public class CosmosDbModule : DatabaseModule {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<CosmosDbConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<CosmosDbServiceClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
