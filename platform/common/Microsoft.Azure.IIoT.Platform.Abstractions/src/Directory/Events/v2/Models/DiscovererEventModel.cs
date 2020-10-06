// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Directory.Models;

    /// <summary>
    /// Discoverer event
    /// </summary>
    public class DiscovererEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public DiscovererEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public DirectoryOperationContextModel Context { get; set; }

        /// <summary>
        /// Discoverer id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Discoverer
        /// </summary>
        public DiscovererModel Discoverer { get; set; }
    }
}