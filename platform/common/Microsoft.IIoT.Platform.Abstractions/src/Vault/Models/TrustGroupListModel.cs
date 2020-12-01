// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Trust group identifier list model
    /// </summary>
    public sealed class TrustGroupListModel {

        /// <summary>
        /// Policies
        /// </summary>
        public IReadOnlyList<string> Groups { get; set; }

        /// <summary>
        /// Next link
        /// </summary>
        public string NextPageLink { get; set; }
    }
}
