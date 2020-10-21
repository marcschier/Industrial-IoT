// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Core.Api.Models {
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// State of the connection
    /// </summary>
    [DataContract]
    public class ConnectionStateApiModel {

        /// <summary>
        /// Last connection state
        /// </summary>
        [DataMember(Name = "state", Order = 0)]
        public ConnectionStatus State { get; set; }

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
