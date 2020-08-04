// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Describes event fields to be published
    /// </summary>
    [DataContract]
    public class PublishedDataSetEventsApiModel {

        /// <summary>
        /// Identifier of event in the dataset.
        /// </summary>
        [DataMember(Name = "id", Order = 0,
            EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Event notifier to subscribe to
        /// </summary>
        [DataMember(Name = "eventNotifier", Order = 1)]
        public string EventNotifier { get; set; }

        /// <summary>
        /// Browse path to event notifier node (Publisher extension)
        /// </summary>
        [DataMember(Name = "browsePath", Order = 2,
            EmitDefaultValue = false)]
        public string[] BrowsePath { get; set; }

        /// <summary>
        /// Event fields to select
        /// </summary>
        [DataMember(Name = "selectedFields", Order = 3)]
        public List<SimpleAttributeOperandApiModel> SelectedFields { get; set; }

        /// <summary>
        /// Filter to use to select event fields
        /// </summary>
        [DataMember(Name = "filter", Order = 4)]
        public ContentFilterApiModel Filter { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        [DataMember(Name = "queueSize", Order = 5,
            EmitDefaultValue = false)]
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        [DataMember(Name = "discardNew", Order = 6,
            EmitDefaultValue = false)]
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        [DataMember(Name = "monitoringMode", Order = 7,
            EmitDefaultValue = false)]
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        [DataMember(Name = "triggerId", Order = 8,
            EmitDefaultValue = false)]
        public string TriggerId { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 9,
           EmitDefaultValue = false)]
        public string GenerationId { get; set; }

        /// <summary>
        /// Events state
        /// </summary>
        [DataMember(Name = "state", Order = 10,
            EmitDefaultValue = false)]
        public PublishedDataSetItemStateApiModel State { get; set; }

        /// <summary>
        /// Last updated
        /// </summary>
        [DataMember(Name = "updated", Order = 11,
            EmitDefaultValue = false)]
        public PublisherOperationContextApiModel Updated { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 12,
            EmitDefaultValue = false)]
        public PublisherOperationContextApiModel Created { get; set; }
    }
}