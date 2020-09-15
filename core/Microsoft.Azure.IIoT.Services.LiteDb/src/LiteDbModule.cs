// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.LiteDb {
    using Microsoft.Azure.IIoT.Services.LiteDb.Clients;
    using Microsoft.Azure.IIoT.Storage.Default;
    using Autofac;

    /// <summary>
    /// Injected LiteDb client
    /// </summary>
    public class LiteDbModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<ItemContainerFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<LiteDbClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
