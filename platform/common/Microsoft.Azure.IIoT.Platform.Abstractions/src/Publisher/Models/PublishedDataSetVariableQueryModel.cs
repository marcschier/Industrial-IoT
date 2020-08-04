// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Query data set variables
    /// </summary>
    public class PublishedDataSetVariableQueryModel {

        /// <summary>
        /// Node id of the variable
        /// </summary>
        public string PublishedVariableNodeId { get; set; }

        /// <summary>
        /// Display name of the published variable
        /// </summary>
        public string PublishedVariableDisplayName { get; set; }

        /// <summary>
        /// Default is <see cref="NodeAttribute.Value"/>.
        /// </summary>
        public NodeAttribute? Attribute { get; set; }
    }
}