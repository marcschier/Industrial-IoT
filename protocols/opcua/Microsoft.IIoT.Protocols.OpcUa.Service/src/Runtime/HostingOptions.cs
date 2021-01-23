// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;
    using Microsoft.IIoT.Azure.EventHub.Processor;

    /// <summary>
    /// Web service configuration
    /// </summary>
    public class HostingOptions : ConfigureOptionBase<WebHostOptions>,
        IConfigureOptions<EventHubConsumerOptions>,
        IConfigureOptions<EventProcessorHostOptions>,
        IConfigureOptions<EventProcessorFactoryOptions> {

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public HostingOptions(IConfiguration configuration) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public void Configure(EventHubConsumerOptions options) {
            var isMinimumDeployment = GetStringOrDefault(PcsVariable.PCS_DEPLOYMENT_LEVEL)
                .EqualsIgnoreCase("Minimum");
            options.ConsumerGroup = GetStringOrDefault(
            PcsVariable.PCS_EVENTHUB_CONSUMERGROUP_TELEMETRY_UX,
                isMinimumDeployment ? "$default" : "telemetryux");
        }

        /// <inheritdoc/>
        public void Configure(EventProcessorHostOptions options) {
            options.InitialReadFromEnd = true;
        }

        /// <inheritdoc/>
        public void Configure(EventProcessorFactoryOptions options) {
            options.SkipEventsOlderThan = TimeSpan.FromMinutes(5);
            options.CheckpointInterval = TimeSpan.FromMinutes(1);
        }

        /// <inheritdoc/>
        public override void Configure(string name, WebHostOptions options) {
            options.ServicePathBase = GetStringOrDefault(
                PcsVariable.PCS_OPCUA_SERVICE_PATH_BASE);
        }
    }
}
