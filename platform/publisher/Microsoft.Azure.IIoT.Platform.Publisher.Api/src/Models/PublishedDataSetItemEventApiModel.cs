// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Dataset item event
    /// </summary>
    [DataContract]
    public class PublishedDataSetItemEventApiModel {

        /// <summary>
        /// Event type
        /// </summary>
        [DataMember(Name = "eventType", Order = 0)]
        public PublishedDataSetItemEventType EventType { get; set; }

        /// <summary>
        /// Writer id
        /// </summary>
        [DataMember(Name = "dataSetWriterId", Order = 1,
            EmitDefaultValue = false)]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Variable id on delete
        /// </summary>
        [DataMember(Name = "variableId", Order = 2,
            EmitDefaultValue = false)]
        public string VariableId { get; set; }

        /// <summary>
        /// Variable definition if item is a variable definition
        /// </summary>
        [DataMember(Name = "dataSetVariable", Order = 3,
            EmitDefaultValue = false)]
        public PublishedDataSetVariableApiModel DataSetVariable { get; set; }

        /// <summary>
        /// Event definition if event definition event
        /// </summary>
        [DataMember(Name = "eventDataSet", Order = 4,
            EmitDefaultValue = false)]
        public PublishedDataSetEventsApiModel EventDataSet { get; set; }
    }
}