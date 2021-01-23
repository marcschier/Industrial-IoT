// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// History update results
    /// </summary>
    public class HistoryUpdateResultModel {

        /// <summary>
        /// List of results from the update operation
        /// </summary>
        public IReadOnlyList<ServiceResultModel> Results { get; set; }

        /// <summary>
        /// Service result in case of service call error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
