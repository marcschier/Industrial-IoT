// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Directory.Models;

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
        public DirectoryOperationContextModel Context { get; set; }

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