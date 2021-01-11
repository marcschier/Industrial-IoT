// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.AspNetCore.Authentication {
    using Microsoft.AspNetCore.Authorization.Infrastructure;

    /// <summary>
    /// Common crud operations
    /// </summary>
    public static class Operations {

        /// <summary>
        /// Create
        /// </summary>
        public static readonly OperationAuthorizationRequirement Create =
            new OperationAuthorizationRequirement { Name = nameof(Create) };

        /// <summary>
        /// Read
        /// </summary>
        public static readonly OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement { Name = nameof(Read) };

        /// <summary>
        /// Update
        /// </summary>
        public static readonly OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement { Name = nameof(Update) };

        /// <summary>
        /// Delete
        /// </summary>
        public static readonly OperationAuthorizationRequirement Delete =
            new OperationAuthorizationRequirement { Name = nameof(Delete) };
    }
}
