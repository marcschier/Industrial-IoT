﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Writer group information
    /// </summary>
    [DataContract]
    public class WriterGroupInfoApiModel {

        /// <summary>
        /// Dataset writer group identifier
        /// </summary>
        [DataMember(Name = "writerGroupId", Order = 1)]
        public string WriterGroupId { get; set; }

        /// <summary>
        /// Name of the writer group
        /// </summary>
        [DataMember(Name = "name", Order = 2,
            EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Priority of the writer group
        /// </summary>
        [DataMember(Name = "priority", Order = 3,
            EmitDefaultValue = false)]
        public byte? Priority { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 5,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }

        /// <summary>
        /// Network message encoding to generate (publisher extension)
        /// </summary>
        [DataMember(Name = "encoding", Order = 6,
            EmitDefaultValue = false)]
        public NetworkMessageEncoding? Encoding { get; set; }

        /// <summary>
        /// Header layout uri
        /// </summary>
        [DataMember(Name = "headerLayoutProfile", Order = 8,
            EmitDefaultValue = false)]
        public string HeaderLayoutUri { get; set; }

        /// <summary>
        /// Network message configuration
        /// </summary>
        [DataMember(Name = "messageSettings", Order = 9,
            EmitDefaultValue = false)]
        public WriterGroupMessageSettingsApiModel MessageSettings { get; set; }

        /// <summary>
        /// Locales to use
        /// </summary>
        [DataMember(Name = "localeIds", Order = 10,
            EmitDefaultValue = false)]
        public List<string> LocaleIds { get; set; }

        /// <summary>
        /// Security mode
        /// </summary>
        [DataMember(Name = "securityMode", Order = 11,
            EmitDefaultValue = false)]
        public SecurityMode? SecurityMode { get; set; }

        /// <summary>
        /// Security group to use
        /// </summary>
        [DataMember(Name = "securityGroupId", Order = 12,
            EmitDefaultValue = false)]
        public string SecurityGroupId { get; set; }

        /// <summary>
        /// Security key services to use
        /// </summary>
        [DataMember(Name = "securityKeyServices", Order = 13,
            EmitDefaultValue = false)]
        public List<ConnectionApiModel> SecurityKeyServices { get; set; }

        /// <summary>
        /// Max network message size
        /// </summary>
        [DataMember(Name = "maxNetworkMessageSize", Order = 14,
            EmitDefaultValue = false)]
        public uint? MaxNetworkMessageSize { get; set; }

        /// <summary>
        /// Batch buffer size (Publisher extension)
        /// </summary>
        [DataMember(Name = "batchSize", Order = 15,
            EmitDefaultValue = false)]
        public int? BatchSize { get; set; }

        /// <summary>
        /// Publishing interval
        /// </summary>
        [DataMember(Name = "publishingInterval", Order = 16,
            EmitDefaultValue = false)]
        public TimeSpan? PublishingInterval { get; set; }

        /// <summary>
        /// Keep alive time
        /// </summary>
        [DataMember(Name = "keepAliveTime", Order = 17,
            EmitDefaultValue = false)]
        public TimeSpan? KeepAliveTime { get; set; }

        /// <summary>
        /// State of the writer group
        /// </summary>
        [DataMember(Name = "state", Order = 18,
            EmitDefaultValue = false)]
        public WriterGroupStateApiModel State { get; set; }

        /// <summary>
        /// Last updated
        /// </summary>
        [DataMember(Name = "updated", Order = 19,
            EmitDefaultValue = false)]
        public OperationContextApiModel Updated { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 20,
            EmitDefaultValue = false)]
        public OperationContextApiModel Created { get; set; }
    }
}
