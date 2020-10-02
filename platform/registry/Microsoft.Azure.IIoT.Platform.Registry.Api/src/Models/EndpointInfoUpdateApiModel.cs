// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Endpoint update request
    /// </summary>
    [DataContract]
    public class EndpointInfoUpdateApiModel {

        /// <summary>
        /// Generation Id to match
        /// </summary>
        [DataMember(Name = "generationId", Order = 0)]
        [Required]
        public string GenerationId { get; set; }

        /// <summary>
        /// State to update
        /// </summary>
        [DataMember(Name = "activationState", Order = 1,
            EmitDefaultValue = false)]
        public EntityActivationState? ActivationState { get; set; }
    }
}
