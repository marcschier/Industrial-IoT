// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.AspNetCore.Authentication.Clients {
    using Microsoft.IIoT.AspNetCore.Storage;
    using Microsoft.IIoT.Azure.ActiveDirectory;
    using Microsoft.IIoT.Azure.ActiveDirectory.Clients;
    using Microsoft.IIoT.Authentication.Clients;
    using Microsoft.IIoT.Authentication.Clients.Default;
    using Microsoft.IIoT.Authentication.Runtime;
    using Microsoft.IIoT.Authentication;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Http.Auth;
    using Microsoft.IIoT.Storage;
    using Microsoft.IIoT.Storage.Services;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using Autofac;

    /// <summary>
    /// Hybrid web service and unattended authentication
    /// </summary>
    public class HybridAuthentication : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterModule<ActiveDirectorySupport>();

            builder.RegisterType<HttpBearerAuthentication>()
                .AsImplementedInterfaces();
            builder.RegisterType<ClientAuthAggregateConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<DefaultTokenProvider>()
                .AsImplementedInterfaces()
                .IfNotRegistered(typeof(ITokenProvider));

            // Cache tokens in protected cache - by default in memory
            builder.RegisterType<MemoryCache>()
                .AsImplementedInterfaces().InstancePerLifetimeScope()
                .IfNotRegistered(typeof(ICache));
            builder.RegisterType<DistributedProtectedCache>()
                .AsImplementedInterfaces();

            // 1) Pass token through
            builder.RegisterType<PassThroughBearerToken>()
                .AsSelf().AsImplementedInterfaces();

            // 2) Fallback to client auth
            builder.RegisterType<HttpHandlerFactory>()
                .AsImplementedInterfaces();
            builder.RegisterType<ClientCredentialClient>()
                .AsSelf().AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            // 3) Use application auth provider
            builder.RegisterType<AppAuthenticationClient>()
                .AsSelf().AsImplementedInterfaces();

            // Use service to service token source
            builder.RegisterType<HybridTokenSource>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }

        /// <summary>
        /// First try passthrough, then try service client credentials
        /// </summary>
        internal class HybridTokenSource : TokenClientAggregateSource, ITokenSource {
            /// <inheritdoc/>
            public HybridTokenSource(PassThroughBearerToken pt, ClientCredentialClient cc,
                AppAuthenticationClient aa, IEnumerable<ITokenClient> providers, ILogger logger)
                    : base(providers, IIoT.Http.Resource.Platform, logger, pt, cc, aa) {
            }
        }
    }
}
