// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using System;

    /// <summary>
    /// Model upload start result model
    /// </summary>
    public class ModelUploadStartResultModel {

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Content type
        /// </summary>
        public string ContentMimeType { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime? TimeStamp { get; set; }
    }

}
