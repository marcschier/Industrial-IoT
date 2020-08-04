// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;

    /// <summary>
    /// Dataset item event
    /// </summary>
    public class PublishedDataSetItemEventModel {

        /// <summary>
        /// Event type
        /// </summary>
        public PublishedDataSetItemEventType EventType { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public PublisherOperationContextModel Context { get; set; }

        /// <summary>
        /// Writer id
        /// </summary>
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Variable id on delete
        /// </summary>
        public string VariableId { get; set; }

        /// <summary>
        /// Variable definition if item is a variable definition
        /// </summary>
        public PublishedDataSetVariableModel DataSetVariable { get; set; }

        /// <summary>
        /// Event definition if event definition event
        /// </summary>
        public PublishedDataSetEventsModel EventDataSet { get; set; }
    }
}