// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka {
    using System;

    /// <summary>
    /// Kafka configuration
    /// </summary>
    public interface IKafkaConsumerConfig : IKafkaServerConfig {

        /// <summary>
        /// Consumer group id
        /// </summary>
        string ConsumerGroup { get; }

        /// <summary>
        /// Subscribe to topic (null = all)
        /// </summary>
        string Topic { get; }

        /// <summary>
        /// Receive batch size
        /// </summary>
        int ReceiveBatchSize { get; }

        /// <summary>
        /// Receive timeout
        /// </summary>
        TimeSpan ReceiveTimeout { get; }

        /// <summary>
        /// Whether to read from end or start.
        /// </summary>
        bool InitialReadFromEnd { get; }

        /// <summary>
        /// Set checkpoint interval. null = never.
        /// </summary>
        TimeSpan? CheckpointInterval { get; }

        /// <summary>
        /// Skip all events older than. null = never.
        /// </summary>
        TimeSpan? SkipEventsOlderThan { get; }
    }
}