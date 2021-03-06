// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;

    /// <summary>
    /// Result of unpublish request
    /// </summary>
    public class PublishStopResultModel {

        /// <summary>
        /// Service result in case of error
        /// </summary>
        public ServiceResultModel ErrorInfo { get; set; }
    }
}
