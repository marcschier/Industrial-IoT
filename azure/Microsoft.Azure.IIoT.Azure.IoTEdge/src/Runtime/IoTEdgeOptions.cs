// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge {
    using Microsoft.Azure.IIoT.Messaging;

    /// <summary>
    /// IoT Edge configuration
    /// </summary>
    public class IoTEdgeOptions {

        /// <summary>
        /// IoTEdgeHub connection string
        /// </summary>
        public string EdgeHubConnectionString { get; set; }

        /// <summary>
        /// Bypass cert validation with hub
        /// </summary>
        public bool BypassCertVerification { get; set; }

        /// <summary>
        /// Transports to use
        /// </summary>
        public TransportOption Transport { get; set; }

        /// <summary>
        /// Product name to use
        /// </summary>
        public string Product { get; set; }
    }
}
