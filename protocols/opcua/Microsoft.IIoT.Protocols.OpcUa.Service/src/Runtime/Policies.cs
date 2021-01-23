// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using System;

    /// <summary>
    /// Defines twin service api policies.
    /// </summary>
    public static class Policies {

        /// <summary>
        /// Allowed to read
        /// </summary>
        public const string CanRead =
            nameof(CanRead);

        /// <summary>
        /// Allowed to update or delete
        /// </summary>
        public const string CanWrite =
            nameof(CanWrite);

        /// <summary>
        /// Allowed to manage
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
                case CanWrite:
                    return context =>
                        context.User.IsInRole(Roles.Write) ||
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.IsInRole(Roles.Sign) ||
                        context.User.HasClaim(c => c.Type == Claims.Execute);
                case CanManage:
                    return context =>
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.IsInRole(Roles.Sign) ||
                        context.User.HasClaim(c => c.Type == Claims.Execute);
                default:
                    return null;
            }
        }
    }
}
