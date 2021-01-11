// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Method argument metadata model
    /// </summary>
    public class MethodMetadataArgumentModel {

        /// <summary>
        /// Name of the argument
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of argument
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data type node of the argument
        /// </summary>
        public NodeModel Type { get; set; }

        /// <summary>
        /// Default value for the argument
        /// </summary>
        public VariantValue DefaultValue { get; set; }

        /// <summary>
        /// Optional, scalar if not set
        /// </summary>
        public NodeValueRank? ValueRank { get; set; }

        /// <summary>
        /// Optional Array dimension of argument
        /// </summary>
        public IReadOnlyList<uint> ArrayDimensions { get; set; }
    }
}
