// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Models {
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
        /// Lost
        /// </summary>
        [EnumMember]
        Lost,

        /// <summary>
        /// Found
        /// </summary>
        [EnumMember]
        Found,

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