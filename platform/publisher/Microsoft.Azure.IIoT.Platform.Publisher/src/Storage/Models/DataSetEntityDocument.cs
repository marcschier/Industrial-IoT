// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Storage.Default {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Dataset entity document
    /// </summary>
    [DataContract]
    public class DataSetEntityDocument {

        /// <summary>
        /// Document type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = ClassTypeName;
        /// <summary/>
        public static readonly string ClassTypeName = "DataSetEntity";

        /// <summary>
        /// Definition type
        /// </summary>
        [DataMember]
        public string Type { get; set; }
        /// <summary/>
        public static readonly string EventSet = nameof(EventSet);
        /// <summary/>
        public static readonly string Variable = nameof(Variable);

        /// <summary>
        /// Published item identifier - defaults to node id if not set.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Variable id in the context of the dataset writer
        /// </summary>
        [DataMember]
        public string VariableId { get; set; }

        /// <summary>
        /// Dataset writer id
        /// </summary>
        [DataMember]
        public string DataSetWriterId { get; set; }

        /// <summary>
        /// Node id of a variable
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// Display name of the published variable
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// An optional component path from the node identified by
        /// PublishedVariableNodeId to the actual node to publish
        /// (Publisher extension).
        /// </summary>
        [DataMember]
        public string[] BrowsePath { get; set; }

        /// <summary>
        /// Default is <see cref="NodeAttribute.Value"/>.
        /// </summary>
        [DataMember]
        public NodeAttribute? Attribute { get; set; }

        /// <summary>
        /// Index range
        /// </summary>
        [DataMember]
        public string IndexRange { get; set; }

        /// <summary>
        /// Sampling Interval hint - default is best effort
        /// </summary>
        [DataMember]
        public TimeSpan? SamplingInterval { get; set; }

        /// <summary>
        /// Data change filter
        /// </summary>
        [DataMember]
        public DataChangeTriggerType? DataChangeFilter { get; set; }

        /// <summary>
        /// Deadband type
        /// </summary>
        [DataMember]
        public DeadbandType? DeadbandType { get; set; }

        /// <summary>
        /// Deadband value
        /// </summary>
        [DataMember]
        public double? DeadbandValue { get; set; }

        /// <summary>
        /// Substitution value for bad / empty results (not supported yet)
        /// </summary>
        [DataMember]
        public VariantValue SubstituteValue { get; set; }

        /// <summary>
        /// MetaData properties qualified names. (not supported yet)
        /// </summary>
        [DataMember]
        public List<string> MetaDataProperties { get; set; }

        /// <summary>
        /// Monitoring mode (Publisher extension)
        /// </summary>
        [DataMember]
        public MonitoringMode? MonitoringMode { get; set; }

        /// <summary>
        /// Queue size (Publisher extension)
        /// </summary>
        [DataMember]
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Discard new values if queue is full (Publisher extension)
        /// </summary>
        [DataMember]
        public bool? DiscardNew { get; set; }

        /// <summary>
        /// Node in dataset writer that triggers reporting
        /// (Publisher extension)
        /// </summary>
        [DataMember]
        public string TriggerId { get; set; }

        /// <summary>
        /// Hidden trigger that triggers reporting this variable on
        /// at a minimum interval.  Mutually exclusive with TriggerId.
        /// (Publisher extension)
        /// </summary>
        [DataMember]
        public TimeSpan? HeartbeatInterval { get; set; }

        /// <summary>
        /// Event notifier to subscribe to (or start node)
        /// </summary>
        [DataMember]
        public string EventNotifier { get; set; }

        /// <summary>
        /// Fields to select
        /// </summary>
        [DataMember]
        public List<SimpleAttributeOperandModel> SelectedFields { get; set; }

        /// <summary>
        /// Filter to use
        /// </summary>
        [DataMember]
        public List<ContentFilterElementModel> FilterElements { get; set; }

        /// <summary>
        /// Order
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Updated at
        /// </summary>
        [DataMember]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// Updated authority
        /// </summary>
        [DataMember]
        public string UpdatedAuditId { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        [DataMember]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Created authority
        /// </summary>
        [DataMember]
        public string CreatedAuditId { get; set; }

        /// <summary>
        /// Error code - if null operation succeeded.
        /// </summary>
        [DataMember]
        public uint? LastResultStatusCode { get; set; }

        /// <summary>
        /// Error message in case of error or null.
        /// </summary>
        [DataMember]
        public string LastResultErrorMessage { get; set; }

        /// <summary>
        /// Additional diagnostics information
        /// </summary>
        [DataMember]
        public VariantValue LastResultDiagnostics { get; set; }

        /// <summary>
        /// Last result change
        /// </summary>
        [DataMember]
        public DateTime? LastResultChange { get; set; }

        /// <summary>
        /// Assigned server identifier
        /// </summary>
        [DataMember]
        public uint? ServerId { get; set; }

        /// <summary>
        /// Assigned client identifier
        /// </summary>
        [DataMember]
        public uint? ClientId { get; set; }

        /// <summary>
        /// Etag
        /// </summary>
        [DataMember(Name = "_etag")]
        public string ETag { get; set; }
    }
}