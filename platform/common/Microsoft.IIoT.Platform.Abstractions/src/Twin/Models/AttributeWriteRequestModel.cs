// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Models {
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Serializers;

    /// <summary>
    /// Attribute and value to write to it
    /// </summary>
    public class AttributeWriteRequestModel {

        /// <summary>
        /// Node to write to (mandatory)
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Attribute to write (mandatory)
        /// </summary>
        public NodeAttribute Attribute { get; set; }

        /// <summary>
        /// Value to write (mandatory)
        /// </summary>
        public VariantValue Value { get; set; }
    }
}
