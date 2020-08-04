// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Serializers;
    using System;

    /// <summary>
    /// A data set variable update request
    /// </summary>
    public class DataSetUpdateVariableRequestModel {

        /// <summary>
        /// Generation id
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// Display name of the published variable
        /// </summary>
        public string PublishedVariableDisplayName { get; set; }

        /// <summary>
        /// Sampling Interval hint - default is best effort
        /// </summary>
        public TimeSpan? SamplingInterval { get; set; }

        /// <summary>
        /// Data change filter
        /// </summary>
        public DataChangeTriggerType? DataChangeFilter { get; set; }

        /// <summary>
        /// Deadband type
        /// </summary>
        public DeadbandType? DeadbandType { get; set; }

        /// <summary>
        /// Deadband value
        /// </summary>
        public double? DeadbandValue { get; set; }

        /// <summary>
        /// Substitution value for bad / empty results (not supported yet)
        /// </summary>
        public VariantValue SubstituteValue { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        public string TriggerId { get; set; }

        /// <summary>
        /// Triggers reporting this variable on a minimum interval.
        /// (Publisher extension)
        /// </summary>
        public TimeSpan? HeartbeatInterval { get; set; }
    }
}