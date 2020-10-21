﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// State of the dataset writer source
    /// </summary>
    [DataContract]
    public class PublishedDataSetSourceStateApiModel {

        /// <summary>
        /// Current connection state
        /// </summary>
        [DataMember(Name = "connectionState", Order = 0,
            EmitDefaultValue = false)]
        public ConnectionStateApiModel ConnectionState { get; set; }

        /// <summary>
        /// Last operation result
        /// </summary>
        [DataMember(Name = "lastResult", Order = 1,
            EmitDefaultValue = false)]
        public ServiceResultApiModel LastResult { get; set; }

        /// <summary>
        /// Last result change
        /// </summary>
        [DataMember(Name = "lastResultChange", Order = 2,
            EmitDefaultValue = false)]
        public DateTime? LastResultChange { get; set; }
    }
}