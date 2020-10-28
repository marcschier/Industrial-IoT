// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Orleans {
    using Autofac;

    /// <summary>
    /// Injected orleans event bus
    /// </summary>
    public class OrleansEventBusModule : OrleansSiloModule {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {
            base.Load(builder); // Start silo first

            // Register client
            builder.RegisterModule<OrleansEventBusClientModule>();
        }
    }
}
