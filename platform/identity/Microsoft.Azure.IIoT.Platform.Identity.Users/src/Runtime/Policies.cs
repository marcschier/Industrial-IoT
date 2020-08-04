// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Users.Auth {
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using System;

    /// <summary>
    /// Defines publisher service api policies.
    /// </summary>
    public static class Policies {

        /// <summary>
        /// Allowed to view
        /// </summary>
        public const string CanRead =
            nameof(CanRead);

        /// <summary>
        /// Allowed to request publish
        /// </summary>
        public const string CanManage =
            nameof(CanManage);

        /// <summary>
        /// Get rights for policy
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        internal static Func<AuthorizationHandlerContext, bool> RoleMapping(string policy) {
            switch (policy) {
                case CanManage:
                    return context =>
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.HasClaim(c => c.Type == Claims.Execute);
                default:
                    return null;
            }
        }
    }
}
