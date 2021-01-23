// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Result of node browse
    /// </summary>
    public class BrowseResultModel {

        /// <summary>
        /// Node info for the currently browsed node
        /// </summary>
        public NodeModel Node { get; set; }

        /// <summary>
        /// References, if included, otherwise null.
        /// </summary>
        public IReadOnlyList<NodeReferenceModel> References { get; set; }

        /// <summary>
        /// Continuation token if more results pending.
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
