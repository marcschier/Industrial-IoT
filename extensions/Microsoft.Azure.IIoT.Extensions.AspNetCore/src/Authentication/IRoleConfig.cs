// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.AspNetCore.Authentication {

    /// <summary>
    /// Role configuration
    /// </summary>
    public interface IRoleConfig {

        /// <summary>
        /// Using roles
        /// </summary>
        bool UseRoles { get; }
    }
}