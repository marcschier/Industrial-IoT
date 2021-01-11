// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ActiveDirectory.Clients {
    using Microsoft.IIoT.Azure.ActiveDirectory.Runtime;
    using Microsoft.IIoT.Extensions.Authentication;
    using Microsoft.IIoT.Extensions.Authentication.Clients;
    using Microsoft.IIoT.Extensions.Authentication.Runtime;
    using Microsoft.IIoT.Extensions.Storage.Services;
    using System.Collections.Generic;
    using Autofac;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Keyvault authentication support using managed service identity,
    /// service principal configuration or local development (in order)
    /// </summary>
    public class KeyVaultAuthentication : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<ClientAuthAggregateConfig>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<MsiKeyVaultClientConfig>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AadSpKeyVaultConfig>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AadUserKeyVaultConfig>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<MsiAuthenticationClient>()
                .AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AppAuthenticationClient>()
                .AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<DevAuthenticationClient>()
                .AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<KeyVaultTokenSource>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<MemoryCache>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<CachingTokenProvider>()
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            base.Load(builder);
        }

        /// <summary>
        /// Authenticate with device token after trying app and developer authentication.
        /// </summary>
        internal class KeyVaultTokenSource : TokenClientAggregateSource, ITokenSource {
            /// <inheritdoc/>
            public KeyVaultTokenSource(MsiAuthenticationClient ma, AppAuthenticationClient aa,
                DevAuthenticationClient ld, IEnumerable<ITokenClient> providers, ILogger logger)
                    : base(providers, Extensions.Http.Resource.KeyVault, logger, ma, aa, ld) {
            }
        }
    }
}
