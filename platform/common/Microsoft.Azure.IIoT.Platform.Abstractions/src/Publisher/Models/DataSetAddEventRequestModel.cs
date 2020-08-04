// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// A data set events registration request
    /// </summary>
    public class DataSetAddEventRequestModel {

        /// <summary>
        /// Event notifier to subscribe to (or start node)
        /// </summary>
        public string EventNotifier { get; set; }

        /// <summary>
        /// Browse path to event notifier node (Publisher extension)
        /// </summary>
        public string[] BrowsePath { get; set; }

        /// <summary>
        /// Fields to select
        /// </summary>
        public List<SimpleAttributeOperandModel> SelectedFields { get; set; }

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
    }
}