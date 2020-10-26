// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Discoverer registration model
    /// </summary>
    [DataContract]
    public class DiscovererApiModel {

        /// <summary>
        /// Discoverer id
        /// </summary>
        [DataMember(Name = "id", Order = 0)]
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Current log level
        /// </summary>
        [DataMember(Name = "logLevel", Order = 6,
            EmitDefaultValue = false)]
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Whether the registration is out of sync between
        /// client (module) and server (service) (default: false).
        /// </summary>
        [DataMember(Name = "outOfSync", Order = 7,
            EmitDefaultValue = false)]
        public bool? OutOfSync { get; set; }

        /// <summary>
        /// Whether discoverer is connected on this registration
        /// </summary>
        [DataMember(Name = "connected", Order = 8,
            EmitDefaultValue = false)]
        public bool? Connected { get; set; }

        /// <summary>
        /// The reported version of the discovery module
        /// </summary>
        [DataMember(Name = "version", Order = 9,
            EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 10,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}
