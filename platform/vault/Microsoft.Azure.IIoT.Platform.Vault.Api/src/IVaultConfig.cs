// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Api {

    /// <summary>
    /// Configuration for service
    /// </summary>
    public interface IVaultConfig {

        /// <summary>
        /// Opc registry service url
        /// </summary>
        string OpcUaVaultServiceUrl { get; }
    }
}
