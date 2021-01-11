// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Device Provisioning services runtime configuration
    /// </summary>
    public sealed class ProvisioningServiceConfig : PostConfigureOptionBase<ProvisioningServiceOptions> {

        /// <inheritdoc/>
        public ProvisioningServiceConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ProvisioningServiceOptions options) {
            if (string.IsNullOrEmpty(options.ConnectionString)) {
                options.ConnectionString = GetStringOrDefault(PcsVariable.PCS_DPS_CONNSTRING,
                    GetStringOrDefault("_DPS_CS"));
            }
        }
    }
}
