// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Authentication.Clients {
    using Microsoft.IIoT.Azure.ActiveDirectory;
    using Microsoft.IIoT.Extensions.Authentication.Clients;
    using Microsoft.IIoT.Extensions.Authentication.Runtime;
    using Microsoft.IIoT.Extensions.Http.Auth;
    using Autofac;
    using Microsoft.IIoT.Extensions.Authentication;

    /// <summary>
    /// Default web service authentication
    /// </summary>
    public class WebApiAuthentication : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<HttpBearerAuthentication>()
                .AsImplementedInterfaces();
            builder.RegisterType<ClientAuthAggregateConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<DefaultTokenProvider>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(ITokenProvider));

            builder.RegisterModule<ActiveDirectorySupport>();

            // Pass token through is the only provider here
            builder.RegisterType<PassThroughBearerToken>()
                .AsSelf();
            builder.RegisterType<TokenClientSource<PassThroughBearerToken>>()
                .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}
