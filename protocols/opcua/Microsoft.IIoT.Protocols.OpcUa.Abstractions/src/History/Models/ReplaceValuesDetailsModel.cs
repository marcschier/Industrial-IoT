// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Replace historic data
    /// </summary>
    public class ReplaceValuesDetailsModel {

        /// <summary>
        /// Values to replace
        /// </summary>
        public IReadOnlyList<HistoricValueModel> Values { get; set; }
    }
}
