// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.Datalake {
    using Microsoft.IIoT.Azure.Datalake.Clients;
    using Microsoft.IIoT.Azure.Datalake.Runtime;
    using Autofac;

    /// <summary>
    /// Injected Datalake client
    /// </summary>
    public class DatalakeClientModule : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Cosmos db collection as storage
            builder.RegisterType<DatalakeStorageClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<DatalakeConfig>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
