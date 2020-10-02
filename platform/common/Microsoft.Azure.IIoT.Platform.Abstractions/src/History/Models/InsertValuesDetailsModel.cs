// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Insert historic data
    /// </summary>
    public class InsertValuesDetailsModel {

        /// <summary>
        /// Values to insert
        /// </summary>
        public IReadOnlyList<HistoricValueModel> Values { get; set; }
    }
}
