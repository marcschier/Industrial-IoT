// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Import, update or delete applications in bulk
    /// </summary>
    public interface IApplicationBulkProcessor {

        /// <summary>
        /// Merge applications and endpoints
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="header"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        Task ProcessDiscoveryEventsAsync(string discovererId,
            DiscoveryContextModel header, 
            IEnumerable<DiscoveryResultModel> results);
    }
}
