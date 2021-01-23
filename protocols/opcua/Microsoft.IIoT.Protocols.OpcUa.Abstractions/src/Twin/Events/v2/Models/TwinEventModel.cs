// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Events.v2.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Twin Event model for event bus
    /// </summary>
    public class TwinEventModel {

        /// <summary>
        /// Type of event
        /// </summary>
        public TwinEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Twin info
        /// </summary>
        public TwinInfoModel Twin { get; set; }
    }
}