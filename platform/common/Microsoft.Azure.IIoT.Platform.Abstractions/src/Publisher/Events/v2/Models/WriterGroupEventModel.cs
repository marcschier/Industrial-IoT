// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;

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
        public PublisherOperationContextModel Context { get; set; }

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