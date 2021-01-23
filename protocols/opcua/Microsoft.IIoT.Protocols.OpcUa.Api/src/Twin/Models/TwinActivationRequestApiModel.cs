// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Core.Api.Models;
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Twin activation request
    /// </summary>
    [DataContract]
    public class TwinActivationRequestApiModel {

        /// <summary>
        /// Endpoint identifier (mandatory)
        /// </summary>
        [DataMember(Name = "endpointId", Order = 0)]
        public string EndpointId { get; set; }

        /// <summary>
        /// Optional twin identifier if different from endpoint id.
        /// </summary>
        [DataMember(Name = "id", Order = 1,
            EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// User for user authentication
        /// </summary>
        [DataMember(Name = "user", Order = 2,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Diagnostics
        /// </summary>
        [DataMember(Name = "diagnostics", Order = 3,
            EmitDefaultValue = false)]
        public DiagnosticsApiModel Diagnostics { get; set; }

        /// <summary>
        /// The operation timeout to create sessions.
        /// </summary>
        [DataMember(Name = "operationTimeout", Order = 4,
            EmitDefaultValue = false)]
        public TimeSpan? OperationTimeout { get; set; }
    }
}

