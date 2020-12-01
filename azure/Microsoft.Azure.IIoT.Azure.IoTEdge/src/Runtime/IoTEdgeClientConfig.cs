// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Runtime {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// IoT Edge device or module configuration
    /// </summary>
    internal sealed class IoTEdgeClientConfig : PostConfigureOptionBase<IoTEdgeClientOptions> {

        /// <inheritdoc/>
        public IoTEdgeClientConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, IoTEdgeClientOptions options) {
            if (string.IsNullOrEmpty(options.EdgeHubConnectionString)) {
                options.EdgeHubConnectionString =
                    GetStringOrDefault(nameof(options.EdgeHubConnectionString));
            }
            var bypass = GetBoolOrDefault(nameof(options.BypassCertVerification));
            if (bypass) {
                options.BypassCertVerification = bypass;
            }
            if (options.Transport == 0) {
                options.Transport = (TransportOption)Enum.Parse(typeof(TransportOption),
                    GetStringOrDefault(nameof(options.Transport),
                        nameof(TransportOption.MqttOverTcp)), true);
            }
            if (string.IsNullOrEmpty(options.Product)) {
                options.Product = GetStringOrDefault(nameof(options.Product), "iiot");
            }
        }
    }
}
