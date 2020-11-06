// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Kafka.Runtime {
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Kafka consumer configuration
    /// </summary>
    internal sealed class KafkaConsumerConfig : PostConfigureOptionBase<KafkaConsumerOptions> {

        /// <inheritdoc/>
        public KafkaConsumerConfig(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, KafkaConsumerOptions options) {
            if (options.CheckpointInterval == null) {
                options.CheckpointInterval = TimeSpan.FromMinutes(1);
            }
            if (options.ReceiveBatchSize <= 0) {
                options.ReceiveBatchSize = 50;
            }
            if (options.ReceiveTimeout == TimeSpan.Zero) {
                options.ReceiveTimeout = TimeSpan.FromSeconds(5);
            }
#if DEBUG
            if (options.SkipEventsOlderThan == null) {
                options.SkipEventsOlderThan = TimeSpan.FromMinutes(5);
            }
#endif
            if (string.IsNullOrEmpty(options.ConsumerTopic)) {
                options.ConsumerTopic = 
                    GetStringOrDefault(PcsVariable.PCS_KAFKA_CONSUMER_TOPIC_REGEX);
            }
            if (string.IsNullOrEmpty(options.ConsumerGroup)) {
                options.ConsumerGroup =
                    GetStringOrDefault(PcsVariable.PCS_KAFKA_CONSUMER_GROUP,
                    GetStringOrDefault("PCS_EVENTHUB_CONSUMERGROUP", "$default"));
            }
        }
    }
}
