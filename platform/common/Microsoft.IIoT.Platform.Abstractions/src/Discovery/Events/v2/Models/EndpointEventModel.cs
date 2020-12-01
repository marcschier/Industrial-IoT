// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Events.v2.Models {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Platform.Core.Models;

    /// <summary>
    /// Endpoint Event model for event bus
    /// </summary>
    public class EndpointEventModel {

        /// <summary>
        /// Type of event
        /// </summary>
        public EndpointEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Endpoint info
        /// </summary>
        public EndpointInfoModel Endpoint { get; set; }
    }
}