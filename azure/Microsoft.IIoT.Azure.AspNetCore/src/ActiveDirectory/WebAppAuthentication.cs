// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.AspNetCore.Authentication.Clients {
    using Microsoft.IIoT.Azure.ActiveDirectory;
    using Microsoft.IIoT.Authentication.Clients;
    using Microsoft.IIoT.Authentication.Runtime;
    using Microsoft.IIoT.Authentication;
    using Microsoft.IIoT.Http.Auth;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Storage.Services;
    using Microsoft.IIoT.Storage;
    using Autofac;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    /// <summary>
    /// Default web app authentication
    /// </summary>
    public class WebAppAuthentication : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<HttpHandlerFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<HttpBearerAuthentication>()
                .AsImplementedInterfaces();
            builder.RegisterType<ClientAuthAggregateConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<DefaultTokenProvider>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(ITokenProvider));
            // Cache tokens in memory
            builder.RegisterType<MemoryCache>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(ICache));

            builder.RegisterModule<ActiveDirectorySupport>();

            // 1) Use auth service open id token client
            builder.RegisterType<OpenIdUserTokenClient>()
                .AsSelf().AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            // 2) Use msal user token as fallback
            builder.RegisterType<MsalUserTokenClient>()
                .AsSelf().AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            // Use service to service token source
            builder.RegisterType<UserTokenSource>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }

        /// <summary>
        /// First try passthrough, then try service client credentials
        /// </summary>
        internal class UserTokenSource : TokenClientAggregateSource, ITokenSource {

            /// <inheritdoc/>
            public UserTokenSource(OpenIdUserTokenClient oi, MsalUserTokenClient uc,
                IEnumerable<ITokenClient> providers, ILogger logger)
                : base(providers, IIoT.Http.Resource.Platform, logger, oi, uc) {
            }
        }
    }
}
