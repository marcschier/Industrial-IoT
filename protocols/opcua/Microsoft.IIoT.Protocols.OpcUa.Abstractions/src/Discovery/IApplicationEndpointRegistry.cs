// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Application endpoint references
    /// </summary>
    public interface IApplicationEndpointRegistry {

        /// <summary>
        /// Get application endpoints
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEnumerable<EndpointInfoModel>> GetApplicationEndpointsAsync(
            string applicationId, bool includeDeleted = false,
            CancellationToken ct = default);
    }
}
