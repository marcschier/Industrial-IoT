// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Result of attribute write
    /// </summary>
    public class WriteResultModel {

        /// <summary>
        /// All results of attribute writes
        /// </summary>
        public IReadOnlyList<AttributeWriteResultModel> Results { set; get; }
    }
}
