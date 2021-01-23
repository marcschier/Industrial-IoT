// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Default;
    using Autofac;

    /// <summary>
    /// Injected event broker stubs
    /// </summary>
    public sealed class EventBrokerStubs : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<EventBrokerStubT<ITwinRegistryListener>>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
