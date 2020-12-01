// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Api.Models {
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Connection model
    /// </summary>
    [DataContract]
    public class ConnectionApiModel {

        /// <summary>
        /// Endpoint information
        /// </summary>
        [DataMember(Name = "endpoint", Order = 0)]
        public EndpointApiModel Endpoint { get; set; }

        /// <summary>
        /// Elevation
        /// </summary>
        [DataMember(Name = "user", Order = 1,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Diagnostics configuration
        /// </summary>
        [DataMember(Name = "diagnostics", Order = 2,
             EmitDefaultValue = false)]
        public DiagnosticsApiModel Diagnostics { get; set; }

        /// <summary>
        /// Certificate thumbprint to validate.
        /// </summary>
        [DataMember(Name = "thumbprint", Order = 3,
             EmitDefaultValue = false)]
        public string Thumbprint { get; set; }

        /// <summary>
        /// The operation timeout to create sessions.
        /// </summary>
        [DataMember(Name = "operationTimeout", Order = 4,
             EmitDefaultValue = false)]
        public TimeSpan? OperationTimeout { get; set; }
    }
}