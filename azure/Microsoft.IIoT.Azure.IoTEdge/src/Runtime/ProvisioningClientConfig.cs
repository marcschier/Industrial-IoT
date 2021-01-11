// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Runtime {
    using Microsoft.IIoT.Azure.IoTEdge;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Provisioning configuration
    /// </summary>
    internal sealed class ProvisioningClientConfig : PostConfigureOptionBase<ProvisioningClientOptions> {

        /// <inheritdoc/>
        public ProvisioningClientConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, ProvisioningClientOptions options) {
            if (string.IsNullOrEmpty(options.IdScope)) {
                options.IdScope = GetStringOrDefault(nameof(options.IdScope),
                    GetStringOrDefault(PcsVariable.PCS_DPS_IDSCOPE));
            }
            if (string.IsNullOrEmpty(options.Endpoint)) {
                options.Endpoint = "global.azure-devices-provisioning.net";
            }
        }
    }
}
