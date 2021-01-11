// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {

    /// <summary>
    /// Provisioning options
    /// </summary>
    public class ProvisioningClientOptions {

        /// <summary>
        /// Dps global endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Id scope
        /// </summary>
        public string IdScope { get; set; }
    }
}
