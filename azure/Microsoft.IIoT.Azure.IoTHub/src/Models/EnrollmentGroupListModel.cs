// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Query result with continuation token
    /// </summary>
    public class EnrollmentGroupListModel {

        /// <summary>
        /// Continuation token to use for next call or null
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Result returned
        /// </summary>
        public IEnumerable<EnrollmentGroupModel> Groups { get; set; }
    }
}
