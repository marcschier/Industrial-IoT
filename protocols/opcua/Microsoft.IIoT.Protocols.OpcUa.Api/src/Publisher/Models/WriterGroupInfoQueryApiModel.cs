// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Writer group registration query request
    /// </summary>
    [DataContract]
    public class WriterGroupInfoQueryApiModel {

        /// <summary>
        /// Return groups with this name
        /// </summary>
        [DataMember(Name = "name", Order = 0,
            EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// With the specified group version
        /// </summary>
        [DataMember(Name = "groupVersion", Order = 2,
            EmitDefaultValue = false)]
        public uint? GroupVersion { get; set; }

        /// <summary>
        /// Return groups only with this encoding
        /// </summary>
        [DataMember(Name = "encoding", Order = 3,
            EmitDefaultValue = false)]
        public NetworkMessageEncoding? Encoding { get; set; }

        /// <summary>
        /// Return groups only in the specified state
        /// </summary>
        [DataMember(Name = "state", Order = 5,
            EmitDefaultValue = false)]
        public WriterGroupStatus? State { get; set; }

        /// <summary>
        /// Return groups with this priority
        /// </summary>
        [DataMember(Name = "priority", Order = 6,
            EmitDefaultValue = false)]
        public byte? Priority { get; set; }
    }
}