// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Data set information describing metadata and settings
    /// </summary>
    [DataContract]
    public class PublishedDataSetSourceInfoApiModel {

        /// <summary>
        /// Name of the published dataset in the writer
        /// </summary>
        [DataMember(Name = "name", Order = 0,
            EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Endpoint id
        /// </summary>
        [DataMember(Name = "endpointId", Order = 1,
            EmitDefaultValue = false)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Credential to use if any
        /// </summary>
        [DataMember(Name = "user", Order = 2,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Source state
        /// </summary>
        [DataMember(Name = "state", Order = 3,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceStateApiModel State { get; set; }

        /// <summary>
        /// Subscription settings
        /// </summary>
        [DataMember(Name = "subscriptionSettings", Order = 4,
            EmitDefaultValue = false)]
        public PublishedDataSetSourceSettingsApiModel SubscriptionSettings { get; set; }

        /// <summary>
        /// Operation timeout
        /// </summary>
        [DataMember(Name = "operationTimeout", Order = 5,
            EmitDefaultValue = false)]
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// Diagnostics to use on the source
        /// </summary>
        [DataMember(Name = "diagnosticsLevel", Order = 6,
            EmitDefaultValue = false)]
        public DiagnosticsLevel? DiagnosticsLevel { get; set; }

        /// <summary>
        /// Extension fields
        /// </summary>
        [DataMember(Name = "extensionFields", Order = 7,
            EmitDefaultValue = false)]
        public Dictionary<string, string> ExtensionFields { get; set; }
    }
}