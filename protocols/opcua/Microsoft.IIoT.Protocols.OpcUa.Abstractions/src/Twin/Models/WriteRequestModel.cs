// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Request node attribute write
    /// </summary>
    public class WriteRequestModel {

        /// <summary>
        /// Attributes to update
        /// </summary>
        public IReadOnlyList<AttributeWriteRequestModel> Attributes { get; set; }

        /// <summary>
        /// Optional header
        /// </summary>
        public RequestHeaderModel Header { get; set; }
    }
}
