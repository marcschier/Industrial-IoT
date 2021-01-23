// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides network scanning services
    /// </summary>
    public interface INetworkScanner {

        /// <summary>
        /// Current discovery mode
        /// </summary>
        DiscoveryMode Mode { get; }

        /// <summary>
        /// Current configuration
        /// </summary>
        DiscoveryConfigModel Configuration { get; }

        /// <summary>
        /// Configure persistent scanning
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        Task ConfigureAsync(DiscoveryMode mode,
            DiscoveryConfigModel config = null);

        /// <summary>
        /// Scan based on configuration
        /// </summary>
        Task ScanAsync();
    }
}
