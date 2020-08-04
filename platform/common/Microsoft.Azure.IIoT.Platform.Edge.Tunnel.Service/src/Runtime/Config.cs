// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Tunnel.Service.Runtime {
    using Microsoft.Azure.IIoT.Azure.AppInsights;
    using Microsoft.Azure.IIoT.Azure.AppInsights.Runtime;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Runtime;
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.EventHub;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Telemetry processor service configuration
    /// </summary>
    public class Config : DiagnosticsConfig, IEventProcessorHostConfig,
        IEventHubConsumerConfig, IIoTHubConfig, IEventProcessorConfig,
        IMetricServerConfig, IAppInsightsConfig {

        /// <inheritdoc/>
        public string InstrumentationKey => _ai.InstrumentationKey;

        /// <inheritdoc/>
        public string EventHubConnString => _eh.EventHubConnString;
        /// <inheritdoc/>
        public string EventHubPath => _eh.EventHubPath;
        /// <inheritdoc/>
        public bool UseWebsockets => _eh.UseWebsockets;

        /// <inheritdoc/>
        public string ConsumerGroup => GetStringOrDefault(
            PcsVariable.PCS_IOTHUB_EVENTHUB_CONSUMER_GROUP_TUNNEL,
                () => "tunnel");

        /// <inheritdoc/>
        public bool InitialReadFromEnd => true;
        /// <inheritdoc/>
        public TimeSpan? SkipEventsOlderThan => TimeSpan.FromMinutes(5);
        /// <inheritdoc/>
        public int ReceiveBatchSize => _ep.ReceiveBatchSize;
        /// <inheritdoc/>
        public TimeSpan ReceiveTimeout => _ep.ReceiveTimeout;
        /// <inheritdoc/>
        public string LeaseContainerName => _ep.LeaseContainerName;
        /// <inheritdoc/>
        public TimeSpan? CheckpointInterval => _ep.CheckpointInterval;
        /// <inheritdoc/>
        public string EndpointSuffix => _ep.EndpointSuffix;
        /// <inheritdoc/>
        public string AccountName => _ep.AccountName;
        /// <inheritdoc/>
        public string AccountKey => _ep.AccountKey;

        /// <inheritdoc/>
        public string IoTHubConnString => _hub.IoTHubConnString;

        /// <inheritdoc/>
        public int Port => 9504;
        /// <inheritdoc/>
        public string Path => _ms.Path;
        /// <inheritdoc/>
        public bool UseHttps => _ms.UseHttps;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration) : base(configuration) {
            _ep = new EventProcessorConfig(configuration);
            _eh = new IoTHubConsumerConfig(configuration);
            _hub = new IoTHubConfig(configuration);
            _ai = new AppInsightsConfig(configuration);
            _ms = new MetricsServerConfig(configuration);
        }

        private readonly MetricsServerConfig _ms;
        private readonly AppInsightsConfig _ai;
        private readonly EventProcessorConfig _ep;
        private readonly IoTHubConsumerConfig _eh;
        private readonly IoTHubConfig _hub;
    }
}
