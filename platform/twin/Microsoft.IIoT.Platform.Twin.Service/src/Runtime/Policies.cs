// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Twin.Service {
    using Microsoft.IIoT.Extensions.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using System;

    /// <summary>
    /// Defines twin service api policies.
    /// </summary>
    public static class Policies {

        /// <summary>
        /// Allowed to read and browse
        /// </summary>
        public const string CanBrowse =
            nameof(CanBrowse);

        /// <summary>
        /// Allowed to write or execute
        /// </summary>
        public const string CanControl =
            nameof(CanControl);

        /// <summary>
        /// Allowed to request uploads
        /// </summary>
        public const string CanUpload =
            nameof(CanUpload);

        /// <summary>
        /// Get rights for policy
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        internal static Func<AuthorizationHandlerContext, bool> RoleMapping(string policy) {
            switch (policy) {
                case CanControl:
                    return context =>
                        context.User.IsInRole(Roles.Write) ||
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.IsInRole(Roles.Sign) ||
                        context.User.HasClaim(c => c.Type == Claims.Execute);
                case CanUpload:
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
