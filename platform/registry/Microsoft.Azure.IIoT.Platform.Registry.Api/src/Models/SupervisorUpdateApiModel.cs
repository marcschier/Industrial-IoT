// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Supervisor update request
    /// </summary>
    [DataContract]
    public class SupervisorUpdateApiModel {

        /// <summary>
        /// Current log level
        /// </summary>
        [DataMember(Name = "logLevel", Order = 1,
            EmitDefaultValue = false)]
        public TraceLogLevel? LogLevel { get; set; }
    }
}
