// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin info
    /// </summary>
    [DataContract]
    public class TwinInfoApiModel {

        /// <summary>
        /// Twin identifier.
        /// </summary>
        [DataMember(Name = "id", Order = 0)]
        public string Id { get; set; }

        /// <summary>
        /// Endpoint identifier
        /// </summary>
        [DataMember(Name = "endpointId", Order = 1)]
        public string EndpointId { get; set; }

        /// <summary>
        /// User or null if anonymous authentication.
        /// </summary>
        [DataMember(Name = "user", Order = 2,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Diagnostics configuration to use for the twin
        /// </summary>
        [DataMember(Name = "diagnostics", Order = 3,
            EmitDefaultValue = false)]
        public DiagnosticsApiModel Diagnostics { get; set; }

        /// <summary>
        /// The operation timeout for this twin.
        /// </summary>
        [DataMember(Name = "operationTimeout", Order = 4,
            EmitDefaultValue = false)]
        public TimeSpan? OperationTimeout { get; set; }

        /// <summary>
        /// The last connection status of this twin
        /// </summary>
        [DataMember(Name = "connectionState", Order = 5,
            EmitDefaultValue = false)]
        public ConnectionStateApiModel ConnectionState { get; set; }

        /// <summary>
        /// Created
        /// </summary>
        [DataMember(Name = "created", Order = 6)]
        public OperationContextApiModel Created { get; set; }

        /// <summary>
        /// Updated
        /// </summary>
        [DataMember(Name = "updated", Order = 7,
            EmitDefaultValue = false)]
        public OperationContextApiModel Updated { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 8)]
        public string GenerationId { get; set; }
    }
}
