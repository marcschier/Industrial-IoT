// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Request node attribute read or update
    /// </summary>
    public class ReadRequestModel {

        /// <summary>
        /// Attributes to update or read
        /// </summary>
        public IReadOnlyList<AttributeReadRequestModel> Attributes { get; set; }

        /// <summary>
        /// Optional header
        /// </summary>
        public RequestHeaderModel Header { get; set; }
    }
}
