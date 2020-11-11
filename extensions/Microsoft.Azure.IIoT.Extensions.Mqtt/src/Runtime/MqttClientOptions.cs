// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Mqtt {

    /// <summary>
    /// Mqtt client configuration
    /// </summary>
    public class MqttClientOptions {

        /// <summary>
        /// Message sent with retain flag
        /// </summary>
        public bool Retain { get; set; }

        /// <summary>
        /// Quality of service
        /// </summary>
        public byte QoS { get; set; }
    }
}