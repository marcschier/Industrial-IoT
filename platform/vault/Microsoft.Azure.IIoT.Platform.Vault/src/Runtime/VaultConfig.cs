// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class VaultConfig : ConfigBase, IVaultConfig {

        /// <summary>
        /// Vault configuration
        /// </summary>
        private const string kOpcVault_AutoApprove =
            "OpcVault:AutoApprove";

        /// <inheritdoc/>
        public bool AutoApprove => GetBoolOrDefault(
            kOpcVault_AutoApprove);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public VaultConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
