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
        /// Binary encoded sample message
        /// </summary>
        public const string DataSetWriterMessage =
            "application/x-sample-binary-v2";

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
        /// Message contains writer group events
        /// </summary>
        public const string WriterGroupEvents =
            "application/x-writergroup-event-v2-json";

        /// <summary>
        /// Message contains writer events
        /// </summary>
        public const string DataSetWriterEvents =
            "application/x-datasetwriter-event-v2-json";

        /// <summary>
        /// Message contains twin state events
        /// </summary>
        public const string TwinEvents =
            "application/x-twin-event-v2-json";

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
