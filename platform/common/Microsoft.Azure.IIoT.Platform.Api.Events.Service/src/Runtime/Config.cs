// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Api.Events.Service.Runtime {
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Cors;
    using Microsoft.Azure.IIoT.AspNetCore.Cors.Runtime;
    using Microsoft.Azure.IIoT.AspNetCore.Authentication;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting;
    using Microsoft.Azure.IIoT.AspNetCore.Hosting.Runtime;
    using Microsoft.Azure.IIoT.Azure.EventHub.Processor;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Common web service configuration aggregation
    /// </summary>
    public class Config : ConfigBase, IWebHostConfig,
        ICorsConfig, IOpenApiConfig, IHeadersConfig,
        IRoleConfig, IConfigureOptions<EventHubConsumerOptions>,
        IConfigureOptions<EventProcessorHostOptions>,
        IConfigureOptions<EventProcessorFactoryOptions> {

        /// <inheritdoc/>
        public string CorsWhitelist => _cors.CorsWhitelist;
        /// <inheritdoc/>
        public bool CorsEnabled => _cors.CorsEnabled;

        /// <inheritdoc/>
        public int HttpsRedirectPort => _host.HttpsRedirectPort;
        /// <inheritdoc/>
        public string ServicePathBase => GetStringOrDefault(
            PcsVariable.PCS_EVENTS_SERVICE_PATH_BASE,
            () => _host.ServicePathBase);

        /// <inheritdoc/>
        public bool UIEnabled => _openApi.UIEnabled;
        /// <inheritdoc/>
        public bool WithAuth => _openApi.WithAuth;
        /// <inheritdoc/>
        public string OpenApiAppId => _openApi.OpenApiAppId;
        /// <inheritdoc/>
        public string OpenApiAppSecret => _openApi.OpenApiAppSecret;
        /// <inheritdoc/>
        public string OpenApiAuthorizationEndpoint => _openApi.OpenApiAuthorizationEndpoint;
        /// <inheritdoc/>
        public bool UseV2 => _openApi.UseV2;
        /// <inheritdoc/>
        public string OpenApiServerHost => _openApi.OpenApiServerHost;

        /// <inheritdoc/>
        public bool IsMinimumDeployment =>
            GetStringOrDefault(PcsVariable.PCS_DEPLOYMENT_LEVEL)
                .EqualsIgnoreCase("Minimum");


        /// <inheritdoc/>
        public bool AspNetCoreForwardedHeadersEnabled =>
            _fh.AspNetCoreForwardedHeadersEnabled;
        /// <inheritdoc/>
        public int AspNetCoreForwardedHeadersForwardLimit =>
            _fh.AspNetCoreForwardedHeadersForwardLimit;

        /// <inheritdoc/>
        public bool UseRoles => GetBoolOrDefault(PcsVariable.PCS_AUTH_ROLES);


        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration) :
            base(configuration) {

            _openApi = new OpenApiConfig(configuration);
            _host = new WebHostConfig(configuration);
            _cors = new CorsConfig(configuration);
            _fh = new HeadersConfig(configuration);
        }

        /// <inheritdoc/>
        public void Configure(EventHubConsumerOptions options) {
            options.ConsumerGroup = GetStringOrDefault(
            PcsVariable.PCS_EVENTHUB_CONSUMERGROUP_TELEMETRY_UX,
                () => IsMinimumDeployment ? "$default" : "telemetryux");
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


        private readonly OpenApiConfig _openApi;
        private readonly WebHostConfig _host;
        private readonly CorsConfig _cors;
        private readonly HeadersConfig _fh;
    }
}
