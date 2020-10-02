// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Endpoint update request
    /// </summary>
    public class EndpointInfoUpdateModel {

        /// <summary>
        /// Generation Id to match
        /// </summary>
        public string GenerationId { get; set; }

        /// <summary>
        /// Activation state to update
        /// </summary>
        public EntityActivationState? ActivationState { get; set; }

        /// <summary>
        /// Operation audit context
        /// </summary>
        public RegistryOperationContextModel Context { get; set; }
    }
}
