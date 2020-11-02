// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Events.Service.Runtime {
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Storage;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Telemetry processor service configuration
    /// </summary>
    public class Config : ConfigBase, IConfigureOptions<EventHubConsumerOptions>,
        IConfigureOptions<EventProcessorHostOptions>, IConfigureOptions<EventProcessorFactoryOptions>,
        IItemContainerConfig, IConfigureOptions<MetricsServerOptions> {

        /// <inheritdoc/>
        public string ContainerName => "iiot_opc";
        /// <inheritdoc/>
        public string DatabaseName => "iiot_opc";


        /// <inheritdoc/>
        public Config(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public void Configure(EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault(
                PcsVariable.PCS_IOTHUB_EVENTHUB_CONSUMER_GROUP_EVENTS, () => "events");
        }

        /// <inheritdoc/>
        public void Configure(MetricsServerOptions options) {
            options.Port = 9500;
        }

        /// <inheritdoc/>
        public void Configure(EventProcessorHostOptions options) {
            options.InitialReadFromEnd = true;
        }

        /// <inheritdoc/>
        public void Configure(EventProcessorFactoryOptions options) {
            options.SkipEventsOlderThan = TimeSpan.FromMinutes(5);
        }
    }
}
