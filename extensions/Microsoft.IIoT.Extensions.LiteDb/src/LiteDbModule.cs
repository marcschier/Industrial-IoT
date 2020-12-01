// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.LiteDb {
    using Microsoft.IIoT.Extensions.LiteDb.Clients;
    using Microsoft.IIoT.Extensions.LiteDb.Runtime;
    using Microsoft.IIoT.Storage;
    using Autofac;

    /// <summary>
    /// Injected LiteDb client
    /// </summary>
    public class LiteDbModule : DatabaseModule {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<LiteDbClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<LiteDbConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
