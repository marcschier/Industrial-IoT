// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Result of a writer registration
    /// </summary>
    [DataContract]
    public class DataSetWriterAddResponseApiModel {

        /// <summary>
        /// New id writer was registered under
        /// </summary>
        [DataMember(Name = "dataSetWriterId", Order = 0)]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 1)]
        public string GenerationId { get; set; }
    }
}
