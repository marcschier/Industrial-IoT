// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// List of data set writers
    /// </summary>
    [DataContract]
    public class DataSetWriterInfoListApiModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        [DataMember(Name = "continuationToken", Order = 0,
            EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Applications
        /// </summary>
        [DataMember(Name = "dataSetWriters", Order = 1,
            EmitDefaultValue = false)]
        public List<DataSetWriterInfoApiModel> DataSetWriters { get; set; }
    }
}
