// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Dataset item event type
    /// </summary>
    [DataContract]
    public enum PublishedDataSetItemEventType {

        /// <summary>
        /// New variable or event definition added
        /// </summary>
        [EnumMember]
        Added,

        /// <summary>
        /// Updated event or variable definition
        /// </summary>
        [EnumMember]
        Updated,

        /// <summary>
        /// State change
        /// </summary>
        [EnumMember]
        StateChange,

        /// <summary>
        /// Deleted definition
        /// </summary>
        [EnumMember]
        Removed,
    }
}