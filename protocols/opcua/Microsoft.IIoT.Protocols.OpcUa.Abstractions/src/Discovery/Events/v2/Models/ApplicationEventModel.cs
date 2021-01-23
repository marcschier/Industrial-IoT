// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Discovery.Events.v2.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Discovery.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Application event
    /// </summary>
    public class ApplicationEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public ApplicationEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Application
        /// </summary>
        public ApplicationInfoModel Application { get; set; }
    }
}