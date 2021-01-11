// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Models {
    using Microsoft.IIoT.Extensions.Serializers;
    using System.Collections.Generic;

    /// <summary>
    /// Generic query result with continuation token
    /// </summary>
    public class QueryResultModel {

        /// <summary>
        /// Continuation token to use for next call or null
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Result returned
        /// </summary>
        public IEnumerable<VariantValue> Result { get; set; }
    }
}
