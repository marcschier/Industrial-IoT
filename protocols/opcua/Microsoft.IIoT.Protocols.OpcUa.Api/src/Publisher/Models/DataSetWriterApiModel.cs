// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Pub/sub writer
    /// </summary>
    [DataContract]
    public class DataSetWriterApiModel {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        [DataMember(Name = "dataSetWriterId", Order = 0)]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Published dataset inline definition
        /// </summary>
        [DataMember(Name = "dataSet", Order = 1,
            EmitDefaultValue = false)]
        public PublishedDataSetApiModel DataSet { get; set; }

        /// <summary>
        /// Dataset field content mask
        /// </summary>
        [DataMember(Name = "dataSetFieldContentMask", Order = 2,
            EmitDefaultValue = false)]
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Data set message settings
        /// </summary>
        [DataMember(Name = "messageSettings", Order = 3,
            EmitDefaultValue = false)]
        public DataSetWriterMessageSettingsApiModel MessageSettings { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 4,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}