// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.CosmosDb {
    using Microsoft.IIoT.Azure.CosmosDb.Runtime;
    using Microsoft.IIoT.Azure.CosmosDb.Clients;
    using Microsoft.IIoT.Extensions.Storage;
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

            builder.RegisterType<CosmosDbServiceClient>()
                .AsImplementedInterfaces();

            builder.AddOptions();
            builder.RegisterType<CosmosDbConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
