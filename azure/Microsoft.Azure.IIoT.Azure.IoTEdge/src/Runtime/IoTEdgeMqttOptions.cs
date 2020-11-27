// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge {

    /// <summary>
    /// Mqtt configuration
    /// </summary>
    public class IoTEdgeMqttOptions {

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
    }
}
