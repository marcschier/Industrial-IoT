// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventHubConsumerConfig : ConfigBase<EventHubConsumerOptions> {

        /// <inheritdoc/>
        public EventHubConsumerConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, EventHubConsumerOptions options) {
            options.EventHubConnString = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_CONNSTRING,
                () => null);
            options.EventHubPath = GetStringOrDefault(PcsVariable.PCS_EVENTHUB_NAME,
                () => null);
            options.UseWebsockets = GetBoolOrDefault("PCS_EVENTHUB_USE_WEBSOCKET",
                () => GetBoolOrDefault("_WS", () => false));
            options.ConsumerGroup = GetStringOrDefault("PCS_EVENTHUB_CONSUMERGROUP",
                () => "$default");
        }
    }
}
