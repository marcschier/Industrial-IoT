// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Result of a variable removal
    /// </summary>
    public class DataSetRemoveVariableResultModel {

        /// <summary>
        /// Diagnostics information in case of partial success
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}