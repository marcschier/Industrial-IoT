// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.KeyVault {
    using Microsoft.Azure.IIoT.Azure.KeyVault.Clients;
    using Microsoft.Azure.IIoT.Azure.KeyVault.Runtime;
    using Autofac;

    /// <summary>
    /// Keyvault client support
    /// </summary>
    public class KeyVaultClientModule : Module {

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<KeyVaultKeyHandleSerializer>()
                .AsImplementedInterfaces();
            builder.RegisterType<KeyVaultServiceClient>()
                .AsImplementedInterfaces();
            builder.RegisterType<KeyVaultConfig>()
                .AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
