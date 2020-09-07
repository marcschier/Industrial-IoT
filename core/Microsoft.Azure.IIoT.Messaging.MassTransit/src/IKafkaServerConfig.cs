// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka {
    /// <summary>
    /// Kafka configuration
    /// </summary>
    public interface IKafkaServerConfig {

        /// <summary>
        /// Comma seperated bootstrap servers
        /// </summary>
        string BootstrapServers { get; }
    }
}