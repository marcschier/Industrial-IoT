// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Synchronize endpoints in the registry
    /// </summary>
    public interface IEndpointBulkProcessor {

        /// <summary>
        /// Add new endpoints, or merge newly found endpoints with the ones
        /// under the specified application id if id is not null.
        /// </summary>
        /// <param name="found"></param>
        /// <param name="context"></param>
        /// <param name="discovererId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        Task ProcessDiscoveryEventsAsync(IEnumerable<EndpointInfoModel> found,
            DiscoveryContextModel context, string discovererId, string applicationId);
    }
}
