// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    public class KafkaProducerConfig : ConfigBase, IKafkaProducerConfig {

        /// <summary>
        /// Kafka producer configuration
        /// </summary>
        private const string kBootstrapServersKey = "BootstrapServers";

        /// <inheritdoc/>
        public string BootstrapServers => GetStringOrDefault(kBootstrapServersKey,
            () => GetStringOrDefault(PcsVariable.PCS_KAFKA_BOOTSTRAP_SERVERS,
                () => "localhost"));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public KafkaProducerConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}
