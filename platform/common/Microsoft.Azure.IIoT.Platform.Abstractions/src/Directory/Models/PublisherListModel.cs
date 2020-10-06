// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Publisher list
    /// </summary>
    public class PublisherListModel {

        /// <summary>
        /// Continuation or null if final
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Publisher items
        /// </summary>
        public IReadOnlyList<PublisherModel> Items { get; set; }
    }
}
