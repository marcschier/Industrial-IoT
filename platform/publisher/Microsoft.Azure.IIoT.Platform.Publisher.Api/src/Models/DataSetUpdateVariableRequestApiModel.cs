// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// A data set variable update request
    /// </summary>
    [DataContract]
    public class DataSetUpdateVariableRequestApiModel {

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        public string GenerationId { get; set; }

        /// <summary>
        /// Display name of the published variable
        /// </summary>
        [DataMember(Name = "publishedVariableDisplayName", Order = 1,
            EmitDefaultValue = false)]
        public string PublishedVariableDisplayName { get; set; }

        /// <summary>
        /// Sampling Interval hint - default is best effort
        /// </summary>
        [DataMember(Name = "samplingInterval", Order = 2,
            EmitDefaultValue = false)]
        public TimeSpan? SamplingInterval { get; set; }

        /// <summary>
        /// Data change filter
        /// </summary>
        [DataMember(Name = "dataChangeFilter", Order = 3,
            EmitDefaultValue = false)]
        public DataChangeTriggerType? DataChangeFilter { get; set; }

        /// <summary>
        /// Deadband type
        /// </summary>
        [DataMember(Name = "deadbandType", Order = 4,
            EmitDefaultValue = false)]
        public DeadbandType? DeadbandType { get; set; }

        /// <summary>
        /// Deadband value
        /// </summary>
        [DataMember(Name = "deadbandValue", Order = 5,
            EmitDefaultValue = false)]
        public double? DeadbandValue { get; set; }

        /// <summary>
        /// Substitution value for bad / empty results (not supported yet)
        /// </summary>
        [DataMember(Name = "substituteValue", Order = 6,
            EmitDefaultValue = false)]
        public VariantValue SubstituteValue { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        [DataMember(Name = "monitoringMode", Order = 7,
            EmitDefaultValue = false)]
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        [DataMember(Name = "queueSize", Order = 8,
            EmitDefaultValue = false)]
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        [DataMember(Name = "discardNew", Order = 9,
            EmitDefaultValue = false)]
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        [DataMember(Name = "triggerId", Order = 10,
            EmitDefaultValue = false)]
        public string TriggerId { get; set; }

        /// <summary>
        /// Triggers reporting this variable on a minimum interval.
        /// (Publisher extension)
        /// </summary>
        [DataMember(Name = "heartbeatInterval", Order = 11,
            EmitDefaultValue = false)]
        public TimeSpan? HeartbeatInterval { get; set; }
    }
}