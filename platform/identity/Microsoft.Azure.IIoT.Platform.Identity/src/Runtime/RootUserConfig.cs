// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Identity.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Root user configuration
    /// </summary>
    internal sealed class RootUserConfig : PostConfigureOptionBase<RootUserOptions> {

        /// <inheritdoc/>
        public RootUserConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, RootUserOptions options) {
            if (string.IsNullOrEmpty(options.UserName)) {
                options.UserName = GetStringOrDefault(PcsVariable.PCS_ROOT_USERID);
            }
            if (string.IsNullOrEmpty(options.Password)) {
                options.Password = GetStringOrDefault(PcsVariable.PCS_ROOT_PASSWORD);
            }
        }
    }
}
