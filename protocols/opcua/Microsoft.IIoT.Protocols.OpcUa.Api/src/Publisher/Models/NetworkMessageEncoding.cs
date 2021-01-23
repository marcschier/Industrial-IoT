﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Message encoding
    /// </summary>
    [DataContract]
    public enum NetworkMessageEncoding {

        /// <summary>
        /// Ua Json encoding
        /// </summary>
        [EnumMember]
        Json = 1,

        /// <summary>
        /// Uadp encoding
        /// </summary>
        [EnumMember]
        Uadp,
    }
}
