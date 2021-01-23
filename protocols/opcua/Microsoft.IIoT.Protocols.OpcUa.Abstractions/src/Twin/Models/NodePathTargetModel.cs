// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Node path target
    /// </summary>
    public class NodePathTargetModel {

        /// <summary>
        /// The target browse path
        /// </summary>
        public IReadOnlyList<string> BrowsePath { get; set; }

        /// <summary>
        /// Target node
        /// </summary>
        public NodeModel Target { get; set; }

        /// <summary>
        /// Remaining index in path
        /// </summary>
        public int? RemainingPathIndex { get; set; }
    }
}
