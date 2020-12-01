// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Request value write
    /// </summary>
    public class ValueWriteRequestModel {

        /// <summary>
        /// Node id to write to - from browse.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// An optional path from NodeId instance to
        /// an actual node.
        /// </summary>
        public IReadOnlyList<string> BrowsePath { get; set; }

        /// <summary>
        /// The data type of the value - from browse.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Index range to write
        /// </summary>
        public string IndexRange { get; set; }

        /// <summary>
        /// Value to write
        /// </summary>
        public VariantValue Value { get; set; }

        /// <summary>
        /// Optional header
        /// </summary>
        public RequestHeaderModel Header { get; set; }
    }
}
