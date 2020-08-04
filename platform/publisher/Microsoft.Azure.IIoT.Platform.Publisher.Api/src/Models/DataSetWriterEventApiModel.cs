// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Dataset writer event
    /// </summary>
    [DataContract]
    public class DataSetWriterEventApiModel {

        /// <summary>
        /// Event type
        /// </summary>
        [DataMember(Name = "eventType", Order = 0)]
        public DataSetWriterEventType EventType { get; set; }

        /// <summary>
        /// Writer id
        /// </summary>
        [DataMember(Name = "id", Order = 1,
            EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Writer
        /// </summary>
        [DataMember(Name = "dataSetWriter", Order = 2,
            EmitDefaultValue = false)]
        public DataSetWriterInfoApiModel DataSetWriter { get; set; }
    }
}