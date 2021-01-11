// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Kafka configuration
    /// </summary>
    internal sealed class KafkaServerConfig : PostConfigureOptionBase<KafkaServerOptions> {

        /// <inheritdoc/>
        public KafkaServerConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, KafkaServerOptions options) {
            if (string.IsNullOrEmpty(options.BootstrapServers)) {
                options.BootstrapServers = GetStringOrDefault(
                    PcsVariable.PCS_KAFKA_BOOTSTRAP_SERVERS, "localhost:9092");
            }
            if (options.Partitions == 0) {
                options.Partitions =
                    GetIntOrDefault(PcsVariable.PCS_KAFKA_PARTITION_COUNT, 8);
            }
            if (options.ReplicaFactor == 0) {
                options.ReplicaFactor =
                    GetIntOrDefault(PcsVariable.PCS_KAFKA_REPLICA_FACTOR, 2);
            }
        }
    }
}
