// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin Event model
    /// </summary>
    [DataContract]
    public class TwinEventApiModel {

        /// <summary>
        /// Type of event
        /// </summary>
        [DataMember(Name = "eventType", Order = 0)]
        public TwinEventType EventType { get; set; }

        /// <summary>
        /// Twin info
        /// </summary>
        [DataMember(Name = "twin", Order = 2,
            EmitDefaultValue = false)]
        public TwinInfoApiModel Twin { get; set; }
    }
}