// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Writer group status
    /// </summary>
    [DataContract]
    public enum WriterGroupStatus {

        /// <summary>
        /// Publishing is disabled
        /// </summary>
        [EnumMember]
        Disabled,

        /// <summary>
        /// Publishing is stopped
        /// </summary>
        [EnumMember]
        Pending,

        /// <summary>
        /// Publishing is ongoing
        /// </summary>
        [EnumMember]
        Publishing
    }
}