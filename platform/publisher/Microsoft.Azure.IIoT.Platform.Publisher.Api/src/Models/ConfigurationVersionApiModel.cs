// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Configuration version
    /// </summary>
    [DataContract]
    public class ConfigurationVersionApiModel {

        /// <summary>
        /// Major version
        /// </summary>
        [DataMember(Name = "majorVersion", Order = 0)]
        public uint MajorVersion { get; set; }

        /// <summary>
        /// Minor version
        /// </summary>
        [DataMember(Name = "minorVersion", Order = 1)]
        public uint MinorVersion { get; set; }
    }
}
