// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Event filter
    /// </summary>
    public class EventFilterModel {

        /// <summary>
        /// Select statement
        /// </summary>
        public IReadOnlyList<SimpleAttributeOperandModel> SelectClauses { get; set; }

        /// <summary>
        /// Where clause
        /// </summary>
        public ContentFilterModel WhereClause { get; set; }
    }
}