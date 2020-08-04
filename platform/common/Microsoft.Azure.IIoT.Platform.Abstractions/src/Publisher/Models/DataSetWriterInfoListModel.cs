// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using System.Collections.Generic;

    /// <summary>
    /// List of data set writers
    /// </summary>
    public class DataSetWriterInfoListModel {

        /// <summary>
        /// Continuation token
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Applications
        /// </summary>
        public List<DataSetWriterInfoModel> DataSetWriters { get; set; }
    }
}
