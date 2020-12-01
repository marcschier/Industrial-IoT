﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Storage.Models {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Serializers;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// DataSet Writer document
    /// </summary>
    [DataContract]
    public class DataSetWriterDocument {

        /// <summary>
        /// Document type
        /// </summary>
        [DataMember]
        public string ClassType { get; set; } = IdentityType.DataSetWriter;

        /// <summary>
        /// Identifier of the document
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Dataset writer id
        /// </summary>
        [DataMember]
        public string DataSetWriterId => Id;

        /// <summary>
        /// The group the writer belongs to
        /// </summary>
        [DataMember]
        public string WriterGroupId { get; set; }

        /// <summary>
        /// Name of the published dataset
        /// </summary>
        [DataMember]
        public string DataSetName { get; set; }

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        [DataMember]
        public string EndpointId { get; set; }

        /// <summary>
        /// Type of credential to use for connecting
        /// </summary>
        [DataMember]
        public CredentialType? CredentialType { get; set; }

        /// <summary>
        /// Credential to pass to server.
        /// </summary>
        [DataMember]
        public VariantValue Credential { get; set; }

        /// <summary>
        /// Extension fields
        /// </summary>
        [DataMember]
        public /*IReadOnlyDictionary*/ IDictionary<string, string> ExtensionFields { get; set; }

        /// <summary>
        /// Dataset field content mask
        /// </summary>
        [DataMember]
        public DataSetFieldContentMask? DataSetFieldContentMask { get; set; }

        /// <summary>
        /// Dataset message content
        /// </summary>
        [DataMember]
        public DataSetContentMask? DataSetMessageContentMask { get; set; }

        /// <summary>
        /// Configured size of network message
        /// </summary>
        [DataMember]
        public ushort? ConfiguredSize { get; set; }

        /// <summary>
        /// Uadp metwork message number
        /// </summary>
        [DataMember]
        public ushort? NetworkMessageNumber { get; set; }

        /// <summary>
        /// Uadp dataset offset
        /// </summary>
        [DataMember]
        public ushort? DataSetOffset { get; set; }

        /// <summary>
        /// The operation timeout to create sessions.
        /// </summary>
        [DataMember]
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// Publishing interval of the Subscription
        /// </summary>
        [DataMember]
        public TimeSpan? PublishingInterval { get; set; }

        /// <summary>
        /// Life time of the Subscription
        /// </summary>
        [DataMember]
        public uint? SubscriptionLifeTimeCount { get; set; }

        /// <summary>
        /// Max keep alive count of the Subscription
        /// </summary>
        [DataMember]
        public uint? MaxKeepAliveCount { get; set; }

        /// <summary>
        /// Max notifications per publish (Subscription)
        /// </summary>
        [DataMember]
        public uint? MaxNotificationsPerPublish { get; set; }

        /// <summary>
        /// Priority of the subscription
        /// </summary>
        [DataMember]
        public byte? SubscriptionPriority { get; set; }

        /// <summary>
        /// Triggers automatic monitored items display name discovery
        /// </summary>
        [DataMember]
        public bool? ResolveDisplayName { get; set; }

        /// <summary>
        /// Requested level of connectivity diagnostics.
        /// </summary>
        [DataMember]
        public DiagnosticsLevel? DiagnosticsLevel { get; set; }

        /// <summary>
        /// Updated at
        /// </summary>
        [DataMember]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// Updated Audit identifier
        /// </summary>
        [DataMember]
        public string UpdatedAuditId { get; set; }

        /// <summary>
        /// Created at
        /// </summary>
        [DataMember]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Created Audit identifier
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
        /// Endpoint state
        /// </summary>
        [DataMember]
        public ConnectionStatus? ConnectionState { get; set; }

        /// <summary>
        /// Last Connection Error code - if null operation succeeded.
        /// </summary>
        [DataMember]
        public uint? ConnectionLastResultStatusCode { get; set; }

        /// <summary>
        /// Error message in case of error or null.
        /// </summary>
        [DataMember]
        public string ConnectionLastResultErrorMessage { get; set; }

        /// <summary>
        /// Additional Connection diagnostics information
        /// </summary>
        [DataMember]
        public VariantValue ConnectionLastResultDiagnostics { get; set; }

        /// <summary>
        /// Last Connection result change
        /// </summary>
        [DataMember]
        public DateTime? ConnectionLastResultChange { get; set; }

        /// <summary>
        /// Writer is disabled
        /// </summary>
        [DataMember]
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Etag
        /// </summary>
        [DataMember(Name = "_etag")]
        public string ETag { get; set; }
    }
}