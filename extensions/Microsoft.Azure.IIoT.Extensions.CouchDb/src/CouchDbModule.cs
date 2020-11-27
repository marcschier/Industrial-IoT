// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.CouchDb {
    using Microsoft.Azure.IIoT.Extensions.CouchDb.Runtime;
    using Microsoft.Azure.IIoT.Extensions.CouchDb.Clients;
    using Microsoft.Azure.IIoT.Storage;
    using Autofac;

    /// <summary>
    /// Injected CouchDb client
    /// </summary>
    public class CouchDbModule : DatabaseModule {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<CouchDbClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<CouchDbConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
