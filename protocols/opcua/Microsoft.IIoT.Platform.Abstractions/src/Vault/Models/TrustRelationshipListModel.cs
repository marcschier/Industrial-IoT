﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Trust relations
    /// </summary>
    public class TrustRelationshipListModel {

        /// <summary>
        /// Relationships
        /// </summary>
        public IReadOnlyList<TrustRelationshipModel> Relationships { get; set; }

        /// <summary>
        /// Next link
        /// </summary>
        public string NextPageLink { get; set; }
    }
}