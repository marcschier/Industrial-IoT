// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Runtime {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// IoT Edge device or module configuration
    /// </summary>
    internal sealed class IoTEdgeConfig : ConfigBase<IoTEdgeOptions> {

        /// <inheritdoc/>
        public IoTEdgeConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, IoTEdgeOptions options) {
            options.EdgeHubConnectionString = 
                GetStringOrDefault(nameof(options.EdgeHubConnectionString));
            options.BypassCertVerification =
                GetBoolOrDefault(nameof(options.BypassCertVerification), () => false);
            options.Transport = (TransportOption)Enum.Parse(typeof(TransportOption),
                GetStringOrDefault(nameof(options.Transport), 
                    () => nameof(TransportOption.MqttOverTcp)), true);
            options.Product = 
                GetStringOrDefault(nameof(options.Product), () => "iiot");
        }
    }
}
