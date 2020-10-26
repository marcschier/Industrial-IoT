// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Discovery.Events.v2.Models {

    /// <summary>
    /// Application event type
    /// </summary>
    public enum ApplicationEventType {

        /// <summary>
        /// New
        /// </summary>
        New,

        /// <summary>
        /// Lost
        /// </summary>
        Lost,

        /// <summary>
        /// Found
        /// </summary>
        Found,

        /// <summary>
        /// Updated
        /// </summary>
        Updated,

        /// <summary>
        /// Deleted
        /// </summary>
        Deleted,
    }
}