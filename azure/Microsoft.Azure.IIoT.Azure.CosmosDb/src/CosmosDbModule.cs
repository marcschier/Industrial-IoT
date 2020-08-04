// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.CosmosDb {
    using Microsoft.Azure.IIoT.Azure.CosmosDb.Clients;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Autofac;

    /// <summary>
    /// Injected Cosmos client
    /// </summary>
    public class CosmosDbModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<ItemContainerFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<CosmosDbServiceClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
