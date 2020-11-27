// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Mqtt {

    /// <summary>
    /// Mqtt configuration
    /// </summary>
    public class MqttOptions {

        /// <summary>
        /// Client identity
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Host name of broker
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Broker port
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Credential
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Message sent with retain flag
        /// </summary>
        public bool Retain { get; set; }

        /// <summary>
        /// Queue size for publishing queue
        /// </summary>
        public uint? QueueSize { get; set; }

        /// <summary>
        /// Quality of service
        /// </summary>
        public byte? QoS { get; set; }

        /// <summary>
        /// Whether to use tls
        /// </summary>
        public bool? UseTls { get; set; }

        /// <summary>
        /// Whether to accept any certificate
        /// </summary>
        public bool? AllowUntrustedCertificates { get; set; }
    }
}