// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Connection info list
    /// </summary>
    public class TwinInfoListModel {

        /// <summary>
        /// Continuation or null if final
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Connection infos
        /// </summary>
        public IReadOnlyList<TwinInfoModel> Items { get; set; }
    }
}
