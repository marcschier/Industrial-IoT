// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka {
    /// <summary>
    /// Kafka configuration
    /// </summary>
    public interface IKafkaServerConfig {

        /// <summary>
        /// Comma seperated bootstrap servers
        /// </summary>
        string BootstrapServers { get; }

        /// <summary>
        /// Number of partitions per topic
        /// </summary>
        int Partitions { get; }

        /// <summary>
        /// Replica factor of new topics
        /// </summary>
        int ReplicaFactor { get; }
    }
}