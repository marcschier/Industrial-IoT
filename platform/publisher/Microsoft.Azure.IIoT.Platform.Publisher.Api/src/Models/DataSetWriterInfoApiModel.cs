// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data set writer
    /// </summary>
    [DataContract]
    public class DataSetWriterInfoApiModel {

        /// <summary>
        /// Dataset writer id
        /// </summary>
        [DataMember(Name = "dataSetWriterId", Order = 0)]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Dataset writer group the writer is part of or default
        /// </summary>
        [DataMember(Name = "writerGroupId", Order = 1,
            EmitDefaultValue = false)]
        public string WriterGroupId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 2,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }

        /// <summary>
        /// Dataset information
        /// </summary>
        [DataMember(Name = "dataSet", Order = 3,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceInfoApiModel DataSet { get; set; }

        /// <summary>
        /// Dataset field content mask the writer applies
        /// </summary>
        [DataMember(Name = "dataSetFieldContentMask", Order = 4,
            EmitDefaultValue = false)]
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Data set message settings
        /// </summary>
        [DataMember(Name = "messageSettings", Order = 5,
            EmitDefaultValue = false)]
        public DataSetWriterMessageSettingsApiModel MessageSettings { get; set; }

        /// <summary>
        /// Dataset writer is disabled
        /// </summary>
        [DataMember(Name = "isDisabled", Order = 6,
            EmitDefaultValue = false)]
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// Last updated
        /// </summary>
        [DataMember(Name = "updated", Order = 7,
            EmitDefaultValue = false)]
        public OperationContextApiModel Updated { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 8,
            EmitDefaultValue = false)]
        public OperationContextApiModel Created { get; set; }
    }
}
