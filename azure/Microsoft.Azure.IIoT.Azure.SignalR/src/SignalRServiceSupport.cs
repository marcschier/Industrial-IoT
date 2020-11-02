// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.SignalR {
    using Microsoft.Azure.IIoT.Azure.SignalR.Services;
    using Microsoft.Azure.IIoT.Azure.SignalR.Runtime;
    using Autofac;

    /// <summary>
    /// Injected signalr service support
    /// </summary>
    public class SignalRServiceSupport : Module {

        /// <summary>
        /// Load the module
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder) {

            // Register signalr hub hosts
            builder.RegisterGeneric(typeof(SignalRServiceHost<>))
                .AsImplementedInterfaces();

            builder.RegisterType<SignalRServiceConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
