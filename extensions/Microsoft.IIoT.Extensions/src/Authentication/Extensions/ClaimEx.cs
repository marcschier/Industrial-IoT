// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Security.Claims {
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Model extensions
    /// </summary>
    public static class ClaimExtensions {

        /// <summary>Upn claim constant</summary>
        public const string UpnClaimSchema = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

        /// <summary>
        /// Get user id
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static Claim GetUserIdentifierClaim(this IEnumerable<Claim> claims) {
            // Check for UPN, then if that doesn't exist check for name identity
            return claims.FirstOrDefault(x => string.Equals(x.Type, UpnClaimSchema,
                    StringComparison.OrdinalIgnoreCase)) ??
                claims.FirstOrDefault(x => string.Equals(x.Type, kNameClaimSchema,
                    StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check whether user is in specific domain
        /// </summary>
        /// <param name="userIdentifierClaim"></param>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public static bool IsInDomain(this Claim userIdentifierClaim, string domainName) {
            return userIdentifierClaim.Value?.EndsWith(domainName,
                StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Check for whether claims come from application token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static bool IsApplicationToken(this IEnumerable<Claim> claims) {
            return claims.GetUserIdentifierClaim() == null &&
                claims.FirstOrDefault(x => string.Equals(x.Type, kAppIdClaimName,
                    StringComparison.OrdinalIgnoreCase))?.Value != null &&
                !string.Equals(claims.FirstOrDefault(x => string.Equals(x.Type, kApplicationTokenScope,
                    StringComparison.OrdinalIgnoreCase))?.Value,
                        kUserImpersonation, StringComparison.OrdinalIgnoreCase);
        }

        private const string kNameClaimSchema =
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string kAppIdClaimName =
            "appid";
        private const string kApplicationTokenScope =
            "http://schemas.microsoft.com/identity/claims/scope";
        private const string kUserImpersonation =
            "user_impersonation";
    }
}
