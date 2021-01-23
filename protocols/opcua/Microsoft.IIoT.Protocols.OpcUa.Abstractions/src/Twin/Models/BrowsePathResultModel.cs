// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Result of node browse continuation
    /// </summary>
    public class BrowsePathResultModel {

        /// <summary>
        /// Targets
        /// </summary>
        public IReadOnlyList<NodePathTargetModel> Targets { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
