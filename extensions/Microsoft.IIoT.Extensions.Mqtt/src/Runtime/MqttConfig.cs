// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Mqtt.Runtime {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Mqtt configuration
    /// </summary>
    internal sealed class MqttConfig : PostConfigureOptionBase<MqttOptions> {

        /// <inheritdoc/>
        public MqttConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, MqttOptions options) {
            if (string.IsNullOrEmpty(options.HostName)) {
                options.HostName = "localhost";
            }
            if (options.Port == null) {
                options.Port = 8883;
            }
            if (options.QoS == null || options.QoS > 2) {
                options.QoS = 1;
            }
            if (options.UseTls == null) {
                options.UseTls = options.Port != 1883;
            }
            if (options.ClientId == null) {
                options.ClientId = Guid.NewGuid().ToString();
            }
        }
    }
}
