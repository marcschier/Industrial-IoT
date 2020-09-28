﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Application event type
    /// </summary>
    [DataContract]
    public enum ApplicationEventType {

        /// <summary>
        /// New
        /// </summary>
        [EnumMember]
        New,

        /// <summary>
        /// Updated
        /// </summary>
        [EnumMember]
        Updated,

        /// <summary>
        /// Deleted
        /// </summary>
        [EnumMember]
        Deleted,
    }
}