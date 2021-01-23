// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Data set writer message model
    /// </summary>
    [DataContract]
    public class PublishedDataSetItemMessageApiModel {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        [DataMember(Name = "dataSetWriterId", Order = 0,
            EmitDefaultValue = false)]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Variable identifier if variable
        /// </summary>
        [DataMember(Name = "variableId", Order = 1,
            EmitDefaultValue = false)]
        public string VariableId { get; set; }

        /// <summary>
        /// Publisher's time stamp
        /// </summary>
        [DataMember(Name = "timestamp", Order = 2,
            EmitDefaultValue = false)]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Sequence Number
        /// </summary>
        [DataMember(Name = "sequenceNumber", Order = 3,
            EmitDefaultValue = false)]
        public uint? SequenceNumber { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value", Order = 4,
            EmitDefaultValue = false)]
        public DataValueApiModel Value { get; set; }

        /// <summary>
        /// Extension information
        /// </summary>
        [DataMember(Name = "extensions", Order = 5,
            EmitDefaultValue = false)]
        public IReadOnlyDictionary<string, string> Extensions { get; set; }
    }
}