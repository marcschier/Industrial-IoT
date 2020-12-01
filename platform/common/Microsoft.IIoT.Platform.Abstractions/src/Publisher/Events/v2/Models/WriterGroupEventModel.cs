// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Events.v2.Models {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;

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