// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Event hub configuration - wraps a configuration root
    /// </summary>
    internal sealed class EventHubConsumerConfig : PostConfigureOptionBase<EventHubConsumerOptions> {

        /// <inheritdoc/>
        public EventHubConsumerConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void PostConfigure(string name, EventHubConsumerOptions options) {
            if (string.IsNullOrEmpty(options.EventHubConnString)) {
                options.EventHubConnString =
                    GetStringOrDefault(PcsVariable.PCS_EVENTHUB_CONNSTRING);
            }
            if (string.IsNullOrEmpty(options.EventHubPath)) {
                options.EventHubPath = 
                    GetStringOrDefault(PcsVariable.PCS_EVENTHUB_NAME);
            }
            var useWebSockets = GetBoolOrDefault("PCS_EVENTHUB_USE_WEBSOCKET",
                GetBoolOrDefault("_WS", false));
            if (useWebSockets) {
                options.UseWebsockets = useWebSockets;
            }
            if (string.IsNullOrEmpty(options.ConsumerGroup)) {
                options.ConsumerGroup = 
                    GetStringOrDefault("PCS_EVENTHUB_CONSUMERGROUP", "$default");
            }
        }
    }
}
