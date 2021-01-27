﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models {
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Operation log model
    /// </summary>
    [DataContract]
    public class OperationContextApiModel {

        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "AuthorityId", Order = 0,
            EmitDefaultValue = false)]
        public string AuthorityId { get; set; }

        /// <summary>
        /// Operation time
        /// </summary>
        [DataMember(Name = "Time", Order = 1,
            EmitDefaultValue = false)]
        public DateTime Time { get; set; }
    }
}
