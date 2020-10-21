// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Twin.Default;
    using Autofac;

    /// <summary>
    /// Injected twin event brokers
    /// </summary>
    public class TwinEventBrokers : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<TwinEventBroker>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
