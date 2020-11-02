// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// IoT hub services runtime configuration
    /// </summary>
    public sealed class IoTHubConfig : ConfigBase<IoTHubOptions> {

        /// <inheritdoc/>
        public IoTHubConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, IoTHubOptions options) {
            options.IoTHubConnString = GetStringOrDefault(PcsVariable.PCS_IOTHUB_CONNSTRING,
                () => GetStringOrDefault("_HUB_CS", () => null));
        }
    }
}
