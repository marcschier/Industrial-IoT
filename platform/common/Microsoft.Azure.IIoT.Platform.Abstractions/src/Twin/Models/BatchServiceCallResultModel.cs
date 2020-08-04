// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Models {
    using System.Collections.Generic;

    /// <summary>
    /// Result of batch calls
    /// </summary>
    public class BatchServiceCallResultModel :
        List<ServiceCallResultModel> {
    }
}
