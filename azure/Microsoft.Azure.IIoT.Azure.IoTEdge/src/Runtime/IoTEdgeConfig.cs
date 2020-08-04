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
    public class IoTEdgeConfig : ConfigBase, IIoTEdgeConfig {

        /// <summary>
        /// Module configuration
        /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string EdgeHubConnectionStringKey = "EdgeHubConnectionString";
        public const string BypassCertVerificationKey = "BypassCertVerification";
        public const string TransportKey = "Transport";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>Hub connection string</summary>
        public string EdgeHubConnectionString =>
            GetStringOrDefault(EdgeHubConnectionStringKey);
        /// <summary>Whether to bypass cert validation</summary>
        public bool BypassCertVerification =>
            GetBoolOrDefault(BypassCertVerificationKey, () => false);
        /// <summary>Transports to use</summary>
        public TransportOption Transport => (TransportOption)Enum.Parse(typeof(TransportOption),
            GetStringOrDefault(TransportKey, () => nameof(TransportOption.MqttOverTcp)), true);

        /// <summary>
        /// Create configuration
        /// </summary>
        /// <param name="configuration"></param>
        public IoTEdgeConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
