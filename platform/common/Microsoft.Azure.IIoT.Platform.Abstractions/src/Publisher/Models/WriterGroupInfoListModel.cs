// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using System.Collections.Generic;

    /// <summary>
    /// List of data set writer groups
    /// </summary>
    public class WriterGroupInfoListModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Writer groups
        /// </summary>
        public List<WriterGroupInfoModel> WriterGroups { get; set; }
    }
}
