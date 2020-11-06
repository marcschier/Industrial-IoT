// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.KeyVault.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public sealed class KeyVaultConfig : PostConfigureOptionBase<KeyVaultOptions> {

        /// <inheritdoc/>
        public KeyVaultConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, KeyVaultOptions options) {
            if (string.IsNullOrEmpty(options.KeyVaultBaseUrl)) {
                options.KeyVaultBaseUrl = GetStringOrDefault("KEYVAULT__BASEURL",
                    GetStringOrDefault(PcsVariable.PCS_KEYVAULT_URL)).Trim();
            }
            if (options.KeyVaultIsHsm == null) {
                options.KeyVaultIsHsm = GetBoolOrDefault(PcsVariable.PCS_KEYVAULT_ISHSM, true);
            }
        }
    }
}
