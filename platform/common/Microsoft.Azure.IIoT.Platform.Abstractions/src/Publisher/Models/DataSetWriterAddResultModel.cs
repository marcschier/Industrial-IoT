// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// Result of a writer registration
    /// </summary>
    public class DataSetWriterAddResultModel {

        /// <summary>
        /// New id writer was registered under
        /// </summary>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
