// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Runtime {
    using Microsoft.IIoT.Azure.IoTEdge;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// IoT Edge mqtt configuration
    /// </summary>
    internal sealed class IoTEdgeMqttConfig : PostConfigureOptionBase<IoTEdgeMqttOptions> {

        /// <inheritdoc/>
        public IoTEdgeMqttConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, IoTEdgeMqttOptions options) {
            if (options.QoS == null || options.QoS > 2) {
                options.QoS = 1;
            }
        }
    }
}
