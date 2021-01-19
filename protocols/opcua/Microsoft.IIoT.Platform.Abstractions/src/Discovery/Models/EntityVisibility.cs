﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Models {

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
        Lost,

        /// <summary>
        /// The entity was found in the network
        /// </summary>
        Found
    }
}
