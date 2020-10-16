// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform {

    /// <summary>
    /// Message schema type constants
    /// </summary>
    public static class MessageSchemaTypes {

        /// <summary>
        /// Monitored item message
        /// </summary>
        public const string MonitoredItemMessageJson =
            "application/x-monitored-item-json-v1";

        /// <summary>
        /// Monitored item message binary encoded
        /// </summary>
        public const string MonitoredItemMessageBinary =
            "application/x-monitored-item-uabinary-v1";

        /// <summary>
        /// Moniored Item Message Model using json encoding
        /// </summary>
        public const string MonitoredItemMessageModelJson =
            "application/x-monitored-itemsample-model-json-v1";

        /// <summary>
        /// Json network message
        /// </summary>
        public const string NetworkMessageJson =
            "application/x-network-message-json-v1";

        /// <summary>
        /// Uadp network message
        /// </summary>
        public const string NetworkMessageUadp =
            "application/x-network-message-uadp-v1";

        /// <summary>
        /// Network Message Model using json encoding
        /// </summary>
        public const string NetworkMessageModelJson =
            "application/x-network-message-model-json-v1";

        /// <summary>
        /// Message contains writer events
        /// </summary>
        public const string DataSetWriterEvents =
            "application/x-datasetwriter-event-v2-json";

        /// <summary>
        /// Content is a nodeset
        /// </summary>
        public const string NodeSet =
            "application/x-node-set-v1";

        /// <summary>
        /// Message contains discover requests
        /// </summary>
        public const string DiscoveryRequest =
            "application/x-discovery-request-v2-json";

        /// <summary>
        /// Message contains discovery events
        /// </summary>
        public const string DiscoveryEvents =
            "application/x-discovery-event-v2-json";

        /// <summary>
        /// Message contains discovery progress messages
        /// </summary>
        public const string DiscoveryMessage =
            "application/x-discovery-message-v2-json";
    }
}
