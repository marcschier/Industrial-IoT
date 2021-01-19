// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {
    using System.Collections.Generic;

    /// <summary>
    /// List of registered applications
    /// </summary>
    public class ApplicationInfoListModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public IReadOnlyList<ApplicationInfoModel> Items { get; set; }
    }
}