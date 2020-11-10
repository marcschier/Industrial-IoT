// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Runtime {
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;

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
                PcsVariable.PCS_EVENTS_SERVICE_PATH_BASE);
        }
    }
}