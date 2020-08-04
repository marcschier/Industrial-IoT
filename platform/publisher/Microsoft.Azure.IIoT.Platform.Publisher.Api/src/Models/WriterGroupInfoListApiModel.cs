// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// List of data set writer groups
    /// </summary>
    [DataContract]
    public class WriterGroupInfoListApiModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        [DataMember(Name = "continuationToken", Order = 0,
            EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Writer groups
        /// </summary>
        [DataMember(Name = "writerGroups", Order = 1,
            EmitDefaultValue = false)]
        public List<WriterGroupInfoApiModel> WriterGroups { get; set; }
    }
}
