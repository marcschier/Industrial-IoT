// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api.Models {
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Browse nodes by path
    /// </summary>
    [DataContract]
    public class BrowsePathRequestApiModel {

        /// <summary>
        /// Node to browse from.
        /// (defaults to root folder).
        /// </summary>
        [DataMember(Name = "nodeId", Order = 0,
            EmitDefaultValue = false)]
        public string NodeId { get; set; }

        /// <summary>
        /// The paths to browse from node.
        /// (mandatory)
        /// </summary>
        [DataMember(Name = "browsePaths", Order = 1)]
        [Required]
        public IReadOnlyList<IReadOnlyList<string>> BrowsePaths { get; set; }

        /// <summary>
        /// Whether to read variable values on target nodes.
        /// (default is false)
        /// </summary>
        [DataMember(Name = "readVariableValues", Order = 2,
            EmitDefaultValue = false)]
        public bool? ReadVariableValues { get; set; }

        /// <summary>
        /// Whether to only return the raw node id
        /// information and not read the target node.
        /// (default is false)
        /// </summary>
        [DataMember(Name = "NodeIdsOnly", Order = 3,
            EmitDefaultValue = false)]
        public bool? NodeIdsOnly { get; set; }

        /// <summary>
        /// Optional request header
        /// </summary>
        [DataMember(Name = "header", Order = 4,
            EmitDefaultValue = false)]
        public RequestHeaderApiModel Header { get; set; }
    }
}
