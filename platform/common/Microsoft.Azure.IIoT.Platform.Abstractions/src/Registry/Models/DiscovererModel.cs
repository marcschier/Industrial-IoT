// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Discoverer registration
    /// </summary>
    public class DiscovererModel {

        /// <summary>
        /// Identifier of the discoverer
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Current log level
        /// </summary>
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Whether the registration is out of sync between
        /// client (module) and server (service) (default: false).
        /// </summary>
        public bool? OutOfSync { get; set; }

        /// <summary>
        /// Whether discoverer is connected
        /// </summary>
        public bool? Connected { get; set; }

        /// <summary>
        /// Version information
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        public string GenerationId { get; set; }
    }
}
