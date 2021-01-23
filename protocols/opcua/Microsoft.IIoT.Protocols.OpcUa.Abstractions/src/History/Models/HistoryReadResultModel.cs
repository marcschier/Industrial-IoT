// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// History read results
    /// </summary>
    public class HistoryReadResultModel<T> {

        /// <summary>
        /// History as json encoded extension object
        /// </summary>
        public T History { get; set; }

        /// <summary>
        /// Continuation token if more results pending.
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
