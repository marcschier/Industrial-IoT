// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge {
    using Microsoft.IIoT.Extensions.Messaging;
    using System;

    /// <summary>
    /// IoT Edge client configuration
    /// </summary>
    public class IoTEdgeClientOptions {

        /// <summary>
        /// EdgeHub connection string
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
        /// Default sas token lifetime
        /// </summary>
        public TimeSpan? TokenLifetime { get; set; }

        /// <summary>
        /// Product name to use
        /// </summary>
        public string Product { get; set; }
    }
}
