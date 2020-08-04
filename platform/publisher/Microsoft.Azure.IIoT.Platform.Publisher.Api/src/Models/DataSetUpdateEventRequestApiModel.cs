// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// A data set events update request
    /// </summary>
    [DataContract]
    public class DataSetUpdateEventRequestApiModel {

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        public string GenerationId { get; set; }

        /// <summary>
        /// Fields to select
        /// </summary>
        [DataMember(Name = "selectedFields", Order = 1,
            EmitDefaultValue = false)]
        public List<SimpleAttributeOperandApiModel> SelectedFields { get; set; }

        /// <summary>
        /// Filter to use
        /// </summary>
        [DataMember(Name = "filter", Order = 2,
            EmitDefaultValue = false)]
        public ContentFilterApiModel Filter { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        [DataMember(Name = "queueSize", Order = 3,
            EmitDefaultValue = false)]
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        [DataMember(Name = "discardNew", Order = 4,
            EmitDefaultValue = false)]
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        [DataMember(Name = "monitoringMode", Order = 5,
            EmitDefaultValue = false)]
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        [DataMember(Name = "triggerId", Order = 6,
            EmitDefaultValue = false)]
        public string TriggerId { get; set; }
    }
}