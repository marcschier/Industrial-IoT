// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Models {

    /// <summary>
    /// Visibility of the entity 
    /// </summary>
    public enum EntityVisibility {

        /// <summary>
        /// Unknown visibilty
        /// </summary>
        Unknown,

        /// <summary>
        /// The entity is currently not visible
        /// </summary>
        NotSeen, 

        /// <summary>
        /// The entity was found in the network
        /// </summary>
        Found
    }
}

