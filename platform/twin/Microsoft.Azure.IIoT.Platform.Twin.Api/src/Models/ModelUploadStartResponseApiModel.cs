// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Models {
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Model upload start response
    /// </summary>
    [DataContract]
    public class ModelUploadStartResponseApiModel {

        /// <summary>
        /// File name assigned to the upload request
        /// </summary>
        [DataMember(Name = "fileName", Order = 0,
            EmitDefaultValue = false)]
        public string FileName { get; set; }

        /// <summary>
        /// Content mime type resolved
        /// </summary>
        [DataMember(Name = "contentMimeType", Order = 1,
            EmitDefaultValue = false)]
        public string ContentMimeType { get; set; }

        /// <summary>
        /// Timestamp the upload started
        /// </summary>
        [DataMember(Name = "timeStamp", Order = 2,
            EmitDefaultValue = false)]
        public DateTime? TimeStamp { get; set; }
    }
}
