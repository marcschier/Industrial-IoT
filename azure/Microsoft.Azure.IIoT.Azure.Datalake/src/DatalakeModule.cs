// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.Datalake {
    using Microsoft.Azure.IIoT.Azure.Datalake.Clients;
    using Autofac;

    /// <summary>
    /// Injected Datalake client
    /// </summary>
    public class DataLakeModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<DataLakeStorageClient>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
