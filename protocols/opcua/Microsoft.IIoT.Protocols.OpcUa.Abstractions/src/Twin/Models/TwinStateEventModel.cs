// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using System;

    /// <summary>
    /// Connection state change event
    /// </summary>
    public class TwinStateEventModel {

        /// <summary>
        /// Twin id
        /// </summary>
        public string TwinId { get; set; }

        /// <summary>
        /// Type of event
        /// </summary>
        public TwinStateEventType EventType { get; set; }

        /// <summary>
        /// Timestamp of event
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Result if event is result of service call
        /// </summary>
        public ServiceResultModel LastResult { get; set; }

        /// <summary>
        /// Connection state if state changed
        /// </summary>
        public ConnectionStateModel ConnectionState { get; set; }
    }
}
