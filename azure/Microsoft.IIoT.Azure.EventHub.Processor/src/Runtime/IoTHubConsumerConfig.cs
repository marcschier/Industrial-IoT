// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.EventHub.Processor.Runtime {
    using Microsoft.IIoT.Azure.EventHub.Processor;
    using Microsoft.IIoT.Utils;
    using Microsoft.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// IoT Hub Event processor configuration - wraps a configuration root
    /// </summary>
    public class IoTHubConsumerConfig : ConfigureOptionBase<EventHubConsumerOptions> {

        /// <summary> Event hub connection string </summary>
        internal string EventHubConnString {
            get {
                var ep = GetStringOrDefault(PcsVariable.PCS_IOTHUB_EVENTHUBENDPOINT,
                    GetStringOrDefault("PCS_IOTHUBREACT_HUB_ENDPOINT"));
                if (string.IsNullOrEmpty(ep)) {
                    var cs = GetStringOrDefault("_EH_CS")?.Trim();
                    if (string.IsNullOrEmpty(cs)) {
                        return null;
                    }
                    return cs;
                }
                if (!ConnectionString.TryParse(IoTHubConnString, out var iothub)) {
                    return null;
                }
                if (ep.StartsWith("Endpoint=", StringComparison.InvariantCultureIgnoreCase)) {
                    ep = ep.Remove(0, "Endpoint=".Length);
                }
                return ConnectionString.CreateEventHubConnectionString(ep,
                    iothub.SharedAccessKeyName, iothub.SharedAccessKey).ToString();
            }
        }

        /// <summary>IoT hub connection string</summary>
        internal string IoTHubConnString => GetStringOrDefault(PcsVariable.PCS_IOTHUB_CONNSTRING,
            GetStringOrDefault("_HUB_CS"));

        /// <summary>Hub name</summary>
        internal string IoTHubName {
            get {
                var name = GetStringOrDefault("PCS_IOTHUBREACT_HUB_NAME");
                if (!string.IsNullOrEmpty(name)) {
                    return name;
                }
                try {
                    return ConnectionString.Parse(IoTHubConnString).HubName;
                }
                catch {
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public IoTHubConsumerConfig(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public override void Configure(string name, EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault("PCS_IOTHUB_EVENTHUBCONSUMERGROUP",
                GetStringOrDefault("PCS_IOTHUBREACT_HUB_CONSUMERGROUP", "$default"));
            options.EventHubConnString = EventHubConnString;
            options.EventHubPath = IoTHubName;
            options.UseWebsockets = GetBoolOrDefault("_WS");
        }
    }
}
