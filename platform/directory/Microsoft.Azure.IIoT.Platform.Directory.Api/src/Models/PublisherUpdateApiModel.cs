﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Publisher registration update request
    /// </summary>
    [DataContract]
    public class PublisherUpdateApiModel {

        /// <summary>
        /// Current log level
        /// </summary>
        [DataMember(Name = "logLevel", Order = 2,
            EmitDefaultValue = false)]
        public TraceLogLevel? LogLevel { get; set; }

        /// <summary>
        /// Generation Id
        /// </summary>
        [DataMember(Name = "generationId", Order = 3,
            EmitDefaultValue = false)]
        public string GenerationId { get; set; }
    }
}
