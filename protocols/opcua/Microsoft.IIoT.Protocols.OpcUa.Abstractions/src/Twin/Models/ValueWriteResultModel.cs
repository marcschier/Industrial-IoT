// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Result of value write
    /// </summary>
    public class ValueWriteResultModel {

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
