// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// State of the dataset item
    /// </summary>
    [DataContract]
    public class PublishedDataSetItemStateApiModel {

        /// <summary>
        /// Last operation result
        /// </summary>
        [DataMember(Name = "lastResult", Order = 0,
            EmitDefaultValue = false)]
        public ServiceResultApiModel LastResult { get; set; }

        /// <summary>
        /// Last result change
        /// </summary>
        [DataMember(Name = "lastResultChange", Order = 1,
            EmitDefaultValue = false)]
        public DateTime? LastResultChange { get; set; }

        /// <summary>
        /// Assigned server identifier
        /// </summary>
        [DataMember(Name = "serverId", Order = 2,
            EmitDefaultValue = false)]
        public uint? ServerId { get; set; }

        /// <summary>
        /// Assigned client identifier
        /// </summary>
        [DataMember(Name = "clientId", Order = 3,
            EmitDefaultValue = false)]
        public uint? ClientId { get; set; }
    }
}