// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Registry.Events.v2.Models {
    using Microsoft.IIoT.Platform.Registry.Models;
    using Microsoft.IIoT.Platform.Core.Models;

    /// <summary>
    /// Gateway event
    /// </summary>
    public class GatewayEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public GatewayEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Gateway id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gateway
        /// </summary>
        public GatewayModel Gateway { get; set; }
    }
}