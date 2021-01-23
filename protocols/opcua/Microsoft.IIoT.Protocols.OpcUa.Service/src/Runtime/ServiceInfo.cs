// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Extensions.Hosting;

    /// <summary>
    /// Service information
    /// </summary>
    public class ServiceInfo : IProcessIdentity {

        /// <summary>
        /// ID
        /// </summary>
        public string ServiceId => "OPC_UA";

        /// <summary>
        /// Process id
        /// </summary>
        public string Id => System.Guid.NewGuid().ToString();

        /// <summary>
        /// Name of service
        /// </summary>
        public string Name => "Opc-Ua-Service";

        /// <summary>
        /// Description of service
        /// </summary>
        public string Description => "Industrial IoT OPC UA Service";
    }
}
