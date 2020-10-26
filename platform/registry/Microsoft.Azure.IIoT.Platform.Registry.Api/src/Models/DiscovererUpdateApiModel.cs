// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Discoverer update request
    /// </summary>
    [DataContract]
    public class DiscovererUpdateApiModel {

        /// <summary>
        /// Current log level
        /// </summary>
        [DataMember(Name = "logLevel", Order = 3,
            EmitDefaultValue = false)]
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 4,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}
