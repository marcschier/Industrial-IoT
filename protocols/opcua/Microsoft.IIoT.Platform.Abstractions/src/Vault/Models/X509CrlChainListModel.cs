// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Crl chain list model
    /// </summary>
    public sealed class X509CrlChainListModel {

        /// <summary>
        /// Chain
        /// </summary>
        public IReadOnlyList<X509CrlChainModel> Chains { get; set; }

        /// <summary>
        /// Next link
        /// </summary>
        public string NextPageLink { get; set; }
    }
}