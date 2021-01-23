// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Historic event
    /// </summary>
    [DataContract]
    public class HistoricEventApiModel {

        /// <summary>
        /// The selected fields of the event
        /// </summary>
        [DataMember(Name = "eventFields", Order = 0,
            EmitDefaultValue = false)]
        public IReadOnlyList<VariantValue> EventFields { get; set; }
    }
}
