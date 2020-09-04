// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit.Testing {
    using Microsoft.Azure.IIoT.Messaging.MassTransit;
    using global::MassTransit;
    using global::MassTransit.AutofacIntegration;

    /// <summary>
    /// Injected Memory bus
    /// </summary>
    public class MemoryBusModule : MassTransitModule {

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilderBusConfigurator configure) {
            configure.UsingInMemory((ctx, option) => {
                option.ConfigureEndpoints(ctx);
            });
        }
    }
}
