// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Models {
    using System.Collections.Generic;

    /// <summary>
    /// List of gateway sites
    /// </summary>
    public class GatewaySiteListModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Sites
        /// </summary>
        public IReadOnlyList<string> Sites { get; set; }
    }
}