// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.KeyVault.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public sealed class KeyVaultConfig : ConfigBase<KeyVaultOptions> {

        /// <inheritdoc/>
        public KeyVaultConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, KeyVaultOptions options) {
            options.KeyVaultBaseUrl = GetStringOrDefault("KEYVAULT__BASEURL",
                () => GetStringOrDefault(PcsVariable.PCS_KEYVAULT_URL)).Trim();
            options.KeyVaultIsHsm = GetBoolOrDefault(PcsVariable.PCS_KEYVAULT_ISHSM,
                () => true);
        }
    }
}
