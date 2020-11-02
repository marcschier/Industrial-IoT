// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Processor.Runtime {
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Diagnostics;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Telemetry processor service configuration
    /// </summary>
    public class Config : ConfigBase, IConfigureOptions<EventHubConsumerOptions>,
        IConfigureOptions<MetricsServerOptions> {

        /// <inheritdoc/>
        public Config(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public void Configure(EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault(
                PcsVariable.PCS_IOTHUB_EVENTHUB_CONSUMER_GROUP_TELEMETRY, () => "telemetry");
        }

        /// <inheritdoc/>
        public void Configure(MetricsServerOptions options) {
            options.Port = 9502;
        }
    }
}
