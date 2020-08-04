// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Writer group event type
    /// </summary>
    [DataContract]
    public enum WriterGroupEventType {

        /// <summary>
        /// New
        /// </summary>
        [EnumMember]
        Added,

        /// <summary>
        /// Updated
        /// </summary>
        [EnumMember]
        Updated,

        /// <summary>
        /// State changed
        /// </summary>
        [EnumMember]
        StateChange,

        /// <summary>
        /// Deleted
        /// </summary>
        [EnumMember]
        Removed,
    }
}