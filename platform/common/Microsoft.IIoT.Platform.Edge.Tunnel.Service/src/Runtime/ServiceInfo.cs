// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Edge.Tunnel.Service.Runtime {
    using Microsoft.IIoT.Extensions.Hosting;

    /// <summary>
    /// Service information
    /// </summary>
    public class ServiceInfo : IProcessIdentity {

        /// <summary>
        /// ID
        /// </summary>
        public string ServiceId => "EDGETUNNELHOST";

        /// <summary>
        /// Process id
        /// </summary>
        public string Id => System.Guid.NewGuid().ToString();

        /// <summary>
        /// Name of service
        /// </summary>
        public string Name => "Edge-Tunnel-Host";

        /// <summary>
        /// Description of service
        /// </summary>
        public string Description => "Azure Industrial IoT Edge Tunnel Host";
    }
}
