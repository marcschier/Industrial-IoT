// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {

    /// <summary>
    /// Writer group status
    /// </summary>
    public enum WriterGroupStatus {

        /// <summary>
        /// Publishing is disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// Publishing is stopped
        /// </summary>
        Pending,

        /// <summary>
        /// Publishing is ongoing
        /// </summary>
        Publishing
    }
}