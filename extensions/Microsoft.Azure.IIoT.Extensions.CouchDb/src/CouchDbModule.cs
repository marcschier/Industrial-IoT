// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.CouchDb {
    using Microsoft.Azure.IIoT.Extensions.CouchDb.Clients;
    using Microsoft.Azure.IIoT.Storage.Services;
    using Autofac;

    /// <summary>
    /// Injected CouchDb client
    /// </summary>
    public class CouchDbModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<ItemContainerFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<CouchDbClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
