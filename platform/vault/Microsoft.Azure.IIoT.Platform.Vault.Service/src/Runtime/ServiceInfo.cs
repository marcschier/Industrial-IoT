﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Service {
    using Microsoft.Azure.IIoT.Diagnostics;

    /// <summary>
    /// Service information
    /// </summary>
    public class ServiceInfo : IProcessIdentity {

        /// <summary>
        /// ID
        /// </summary>
        public string ServiceId => "OPC_VAULT";

        /// <summary>
        /// Process id
        /// </summary>
        public string Id => System.Guid.NewGuid().ToString();

        /// <summary>
        /// Name of service
        /// </summary>
        public string Name => "Opc-Vault-Service";

        /// <summary>
        /// Description of service
        /// </summary>
        public string Description => "Azure Industrial IoT OPC UA Vault Service";
    }
}
