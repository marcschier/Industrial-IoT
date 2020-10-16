// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Models {
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Supervisor runtime status
    /// </summary>
    [DataContract]
    public class SupervisorStatusApiModel {

        /// <summary>
        /// Edge device id
        /// </summary>
        [DataMember(Name = "deviceId", Order = 0)]
        [Required]
        public string DeviceId { get; set; }

        /// <summary>
        /// Module id
        /// </summary>
        [DataMember(Name = "moduleId", Order = 1,
            EmitDefaultValue = false)]
        public string ModuleId { get; set; }
    }
}
