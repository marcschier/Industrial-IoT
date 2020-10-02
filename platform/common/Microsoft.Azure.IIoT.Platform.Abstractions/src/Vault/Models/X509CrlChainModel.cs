// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Crl collection model
    /// </summary>
    public sealed class X509CrlChainModel {

        /// <summary>
        /// Chain
        /// </summary>
        public IReadOnlyList<X509CrlModel> Chain { get; set; }
    }
}
