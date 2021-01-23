// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Attribute to read
    /// </summary>
    public class AttributeReadRequestModel {

        /// <summary>
        /// Node to read from (mandatory)
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Attribute to read
        /// </summary>
        public NodeAttribute Attribute { get; set; }
    }
}
