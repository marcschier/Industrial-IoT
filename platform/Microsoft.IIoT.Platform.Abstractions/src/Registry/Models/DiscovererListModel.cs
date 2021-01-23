// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Discoverer list
    /// </summary>
    public class DiscovererListModel {

        /// <summary>
        /// Continuation or null if final
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Discoverers
        /// </summary>
        public IReadOnlyList<DiscovererModel> Items { get; set; }
    }
}
