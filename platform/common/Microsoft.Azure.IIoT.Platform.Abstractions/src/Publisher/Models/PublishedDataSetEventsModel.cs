﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Describes event fields to be published
    /// </summary>
    public class PublishedDataSetEventsModel {

        /// <summary>
        /// Identifier of the data set which is always the dataset writer id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Event notifier node to subscribe to (or start node)
        /// </summary>
        public string EventNotifier { get; set; }

        /// <summary>
        /// Browse path to event notifier node (Publisher extension)
        /// </summary>
        public IReadOnlyList<string> BrowsePath { get; set; }

        /// <summary>
        /// Fields to select
        /// </summary>
        public IReadOnlyList<SimpleAttributeOperandModel> SelectedFields { get; set; }

        /// <summary>
        /// Filter to use
        /// </summary>
        public ContentFilterModel Filter { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        public string TriggerId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// Events state
        /// </summary>
        public PublishedDataSetItemStateModel State { get; set; }

        /// <summary>
        /// Last updated
        /// </summary>
        public PublisherOperationContextModel Updated { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        public PublisherOperationContextModel Created { get; set; }
    }
}