// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Api.Models {
    using Microsoft.IIoT.Platform.Core.Api.Models;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Twin update request
    /// </summary>
    [DataContract]
    public class TwinInfoUpdateApiModel {

        /// <summary>
        /// Generation Id to match
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        public string GenerationId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "user", Order = 1,
            EmitDefaultValue = false)]
        public CredentialApiModel User { get; set; }

        /// <summary>
        /// Diagnostics
        /// </summary>
        [DataMember(Name = "diagnostics", Order = 2,
            EmitDefaultValue = false)]
        public DiagnosticsApiModel Diagnostics { get; set; }

        /// <summary>
        /// The operation timeout to create sessions.
        /// </summary>
        [DataMember(Name = "operationTimeout", Order = 3,
            EmitDefaultValue = false)]
        public TimeSpan? OperationTimeout { get; set; }
    }
}
