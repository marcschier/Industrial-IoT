// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin event type
    /// </summary>
    [DataContract]
    public enum TwinEventType {

        /// <summary>
        /// Activated
        /// </summary>
        [EnumMember]
        Activated,

        /// <summary>
        /// Updated
        /// </summary>
        [EnumMember]
        Updated,

        /// <summary>
        /// Deactivated
        /// </summary>
        [EnumMember]
        Deactivated,
    }
}