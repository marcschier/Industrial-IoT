// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Edge.Events.Service.Runtime {
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Diagnostics;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Telemetry processor service configuration
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<MetricsServerOptions>, 
        IConfigureOptions<EventHubConsumerOptions>,
        IConfigureOptions<EventProcessorHostOptions>, 
        IConfigureOptions<EventProcessorFactoryOptions> {

        /// <inheritdoc/>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, MetricsServerOptions options) {
            options.Port = 9500;
        }

        /// <inheritdoc/>
        public void Configure(EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault(
                PcsVariable.PCS_IOTHUB_EVENTHUB_CONSUMER_GROUP_EVENTS, "events");
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
