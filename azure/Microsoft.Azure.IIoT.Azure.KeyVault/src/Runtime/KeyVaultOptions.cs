// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.KeyVault {

    /// <summary>
    /// Keyvault options
    /// </summary>
    public class KeyVaultOptions {

        /// <summary>
        /// Keyvault base url
        /// </summary>
        public string KeyVaultBaseUrl { get; set; }

        /// <summary>
        /// Is hsm key vault
        /// </summary>
        public bool KeyVaultIsHsm { get; set; }
    }
}