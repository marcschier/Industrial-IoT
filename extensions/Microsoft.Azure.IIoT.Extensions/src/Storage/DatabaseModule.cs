// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage {
    using Microsoft.Azure.IIoT.Storage.Runtime;
    using Microsoft.Azure.IIoT.Storage.Services;
    using Autofac;

    /// <summary>
    /// Database
    /// </summary>
    public class DatabaseModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<CollectionFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<CollectionFactoryConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
