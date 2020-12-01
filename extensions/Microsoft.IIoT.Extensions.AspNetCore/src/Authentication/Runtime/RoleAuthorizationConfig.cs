// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.IIoT.AspNetCore.Authentication {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Role based auth configuration
    /// </summary>
    internal sealed class RoleAuthorizationConfig : PostConfigureOptionBase<RoleAuthorizationOptions> {

        /// <inheritdoc/>
        public RoleAuthorizationConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, RoleAuthorizationOptions options) {
            if (!options.UseRoles) {
                options.UseRoles = GetBoolOrDefault(PcsVariable.PCS_AUTH_ROLES);
            }
        }
    }
}