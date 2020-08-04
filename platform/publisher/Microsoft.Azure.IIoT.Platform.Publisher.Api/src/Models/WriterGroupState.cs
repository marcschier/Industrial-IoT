// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Writer group state
    /// </summary>
    [DataContract]
    public enum WriterGroupState {

        /// <summary>
        /// Publishing is suspended or pending
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