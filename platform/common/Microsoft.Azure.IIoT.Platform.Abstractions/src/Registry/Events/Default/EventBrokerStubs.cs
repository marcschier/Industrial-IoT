﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry {
    using Microsoft.Azure.IIoT.Platform.Registry.Default;
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

            builder.RegisterType<EventBrokerStubT<IEndpointRegistryListener>>()
                .AsImplementedInterfaces();
            builder.RegisterType<EventBrokerStubT<IApplicationRegistryListener>>()
                .AsImplementedInterfaces();
            builder.RegisterType<EventBrokerStubT<IDiscovererProgressProcessor>>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
