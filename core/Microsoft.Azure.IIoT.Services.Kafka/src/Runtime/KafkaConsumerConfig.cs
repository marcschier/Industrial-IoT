// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Runtime {
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Event processor configuration - wraps a configuration root
    /// </summary>
    public class KafkaConsumerConfig : KafkaServerConfig, IKafkaConsumerConfig {

        /// <summary>
        /// Kafka producer configuration
        /// </summary>
        private const string kConsumerTopicKey = "ConsumerTopic";
        private const string kConsumerGroupKey = "ConsumerGroup";
        private const string kReceiveBatchSizeKey = "ReceiveBatchSize";
        private const string kReceiveTimeoutKey = "ReceiveTimeout";
        private const string kInitialReadFromEnd = "InitialReadFromEnd";
        private const string kSkipEventsOlderThanKey = "SkipEventsOlderThan";
        private const string kCheckpointIntervalKey = "CheckpointIntervalKey";

        /// <inheritdoc/>
        public string ConsumerGroup => GetStringOrDefault(kConsumerGroupKey,
            () => GetStringOrDefault(PcsVariable.PCS_KAFKA_CONSUMER_GROUP,
            () => GetStringOrDefault("PCS_EVENTHUB_CONSUMERGROUP",
                () => "$default")));
        /// <inheritdoc/>
        public string ConsumerTopic => GetStringOrDefault(kConsumerTopicKey,
            () => GetStringOrDefault(PcsVariable.PCS_KAFKA_CONSUMER_TOPIC_REGEX,
                () => null));
        /// <inheritdoc/>
        public int ReceiveBatchSize => GetIntOrDefault(kReceiveBatchSizeKey,
            () => 999);
        /// <inheritdoc/>
        public TimeSpan ReceiveTimeout => GetDurationOrDefault(kReceiveTimeoutKey,
            () => TimeSpan.FromSeconds(5));
        /// <inheritdoc/>
        public bool InitialReadFromEnd => GetBoolOrDefault(kInitialReadFromEnd,
            () => false);
        /// <inheritdoc/>
        public TimeSpan? SkipEventsOlderThan => GetDurationOrNull(kSkipEventsOlderThanKey,
#if DEBUG
            () => TimeSpan.FromMinutes(5)); // Skip in debug builds where we always restarted.
#else
            () => null);
#endif
        /// <inheritdoc/>
        public TimeSpan? CheckpointInterval => GetDurationOrDefault(kCheckpointIntervalKey,
            () => TimeSpan.FromMinutes(1));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public KafkaConsumerConfig(IConfiguration configuration = null) :
            base(configuration) {
        }
    }
}
