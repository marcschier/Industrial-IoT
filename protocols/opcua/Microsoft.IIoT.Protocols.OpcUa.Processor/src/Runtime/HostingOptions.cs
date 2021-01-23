// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Publisher.Processor.Runtime {
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Telemetry processor service configuration
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<EventHubConsumerOptions>,
        IConfigureOptions<MetricsServerOptions>,
        IConfigureNamedOptions<MetricsServerOptions> {

        /// <inheritdoc/>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault(
                PcsVariable.PCS_IOTHUB_EVENTHUB_CONSUMER_GROUP_TELEMETRY, "telemetry");
        }

        /// <inheritdoc/>
        public void Configure(string name, MetricsServerOptions options) {
            options.Port = 9502;
        }

        /// <inheritdoc/>
        public void Configure(MetricsServerOptions options) {
            Configure(Options.DefaultName, options);
        }
    }
}
