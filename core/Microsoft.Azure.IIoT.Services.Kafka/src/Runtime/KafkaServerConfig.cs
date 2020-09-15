// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Kafka configuration
    /// </summary>
    public class KafkaServerConfig : ConfigBase, IKafkaServerConfig {

        /// <summary>
        /// Kafka server configuration
        /// </summary>
        private const string kBootstrapServersKey = "BootstrapServers";
        private const string kPartitionsKey = "Partitions";
        private const string kReplicaFactorKey = "ReplicaFactor";

        /// <inheritdoc/>
        public string BootstrapServers => GetStringOrDefault(kBootstrapServersKey,
            () => GetStringOrDefault(PcsVariable.PCS_KAFKA_BOOTSTRAP_SERVERS,
                () => "localhost:9092"));
        /// <inheritdoc/>
        public int Partitions => GetIntOrDefault(kPartitionsKey,
            () => GetIntOrDefault(PcsVariable.PCS_KAFKA_PARTITION_COUNT,
                () => 8));
        /// <inheritdoc/>
        public int ReplicaFactor => GetIntOrDefault(kReplicaFactorKey,
            () => GetIntOrDefault(PcsVariable.PCS_KAFKA_REPLICA_FACTOR,
                () => 2));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public KafkaServerConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
