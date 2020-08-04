// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Events.v2.Models {

    /// <summary>
    /// Dataset item event type
    /// </summary>
    public enum PublishedDataSetItemEventType {

        /// <summary>
        /// New variable or event definition added
        /// </summary>
        Added,

        /// <summary>
        /// Updated event or variable definition
        /// </summary>
        Updated,

        /// <summary>
        /// State change
        /// </summary>
        StateChange,

        /// <summary>
        /// Deleted definition
        /// </summary>
        Removed,
    }
}