// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin info list
    /// </summary>
    [DataContract]
    public class TwinInfoListApiModel {

        /// <summary>
        /// Continuation or null if final
        /// </summary>
        [DataMember(Name = "continuationToken", Order = 0,
           EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Connection infos
        /// </summary>
        [DataMember(Name = "items", Order = 1,
           EmitDefaultValue = false)]
        public IReadOnlyList<TwinInfoApiModel> Items { get; set; }
    }
}
