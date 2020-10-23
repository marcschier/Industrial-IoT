// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api {

    /// <summary>
    /// Event target constants
    /// </summary>
    public static class EventTargets {

        /// <summary>
        /// Supervisor event type
        /// </summary>
        public const string SupervisorEventTarget = "SupervisorEvent";

        /// <summary>
        /// Gateway event type
        /// </summary>
        public const string GatewayEventTarget = "GatewayEvent";

        /// <summary>
        /// Discoverer event type
        /// </summary>
        public const string DiscovererEventTarget = "DiscovererEvent";

        /// <summary>
        /// Publisher event type
        /// </summary>
        public const string PublisherEventTarget = "PublisherEvent";
    }
}
