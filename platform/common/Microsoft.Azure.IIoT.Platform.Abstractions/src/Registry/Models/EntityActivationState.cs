// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Activation state of the entity twin
    /// </summary>
    public enum EntityActivationState {

        /// <summary>
        /// Entity twin is deactivated
        /// </summary>
        Deactivated,

        /// <summary>
        /// Entity twin is activated but not connected
        /// </summary>
        Activated,

        /// <summary>
        /// Entity twin is activated and connected to hub
        /// </summary>
        ActivatedAndConnected
    }
}
