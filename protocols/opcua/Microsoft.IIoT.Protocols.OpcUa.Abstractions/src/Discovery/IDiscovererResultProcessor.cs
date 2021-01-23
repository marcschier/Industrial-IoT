// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Discovery processing
    /// </summary>
    public interface IDiscovererResultProcessor {

        /// <summary>
        /// Consolidate any existing applications with the
        /// provided discovery events list from the supervisor.
        /// </summary>
        /// <param name="discovererId"></param>
        /// <param name="result"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task ProcessDiscoveryResultsAsync(string discovererId,
            DiscoveryContextModel result, IEnumerable<DiscoveryResultModel> events);
    }
}
