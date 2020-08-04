// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Query data set variables
    /// </summary>
    [DataContract]
    public class PublishedDataSetVariableQueryApiModel {

        /// <summary>
        /// Node id of the variable
        /// </summary>
        [DataMember(Name = "publishedVariableNodeId", Order = 0,
            EmitDefaultValue = false)]
        public string PublishedVariableNodeId { get; set; }

        /// <summary>
        /// Display name of the published variable
        /// </summary>
        [DataMember(Name = "publishedVariableDisplayName", Order = 1,
            EmitDefaultValue = false)]
        public string PublishedVariableDisplayName { get; set; }

        /// <summary>
        /// Default is <see cref="NodeAttribute.Value"/>.
        /// </summary>
        [DataMember(Name = "attribute", Order = 2,
            EmitDefaultValue = false)]
        public NodeAttribute? Attribute { get; set; }
    }
}