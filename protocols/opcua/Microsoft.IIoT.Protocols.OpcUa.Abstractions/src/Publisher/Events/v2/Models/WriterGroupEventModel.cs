﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Events.v2.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;

    /// <summary>
    /// Writer group event
    /// </summary>
    public class WriterGroupEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public WriterGroupEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContextModel Context { get; set; }

        /// <summary>
        /// Writer group id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Writer group
        /// </summary>
        public WriterGroupInfoModel WriterGroup { get; set; }
    }
}