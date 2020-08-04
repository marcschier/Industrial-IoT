// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Data set writer query
    /// </summary>
    [DataContract]
    public class DataSetWriterInfoQueryApiModel {

        /// <summary>
        /// Query by name
        /// </summary>
        [DataMember(Name = "dataSetName", Order = 0,
            EmitDefaultValue = false)]
        public string DataSetName { get; set; }

        /// <summary>
        /// Query by endpoint
        /// </summary>
        [DataMember(Name = "endpointId", Order = 1,
            EmitDefaultValue = false)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Dataset writer group.
        /// </summary>
        [DataMember(Name = "writerGroupId", Order = 2,
            EmitDefaultValue = false)]
        public string WriterGroupId { get; set; }
    }
}
