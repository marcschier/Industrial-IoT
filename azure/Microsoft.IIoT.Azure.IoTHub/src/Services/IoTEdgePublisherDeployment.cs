// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Services {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Extensions.Serializers;
    using Microsoft.IIoT.Azure.LogAnalytics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Deploys publisher module
    /// </summary>
    public sealed class IoTEdgePublisherDeployment : IHostProcess {

        /// <summary>
        /// Create deployer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgePublisherDeployment(IDeviceDeploymentServices service,
            IOptions<ContainerRegistryOptions> config, IOptions<LogAnalyticsOptions> diagnostics,
            IJsonSerializer serializer, ILogger logger) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _config = config ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _service.CreateOrUpdateConfigurationAsync(new ConfigurationModel {
                Id = "__default-opcpublisher",
                Content = new ConfigurationContentModel {
                    ModulesContent = CreateLayeredDeployment()
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = IoTEdgeBaseDeployment.TargetCondition,
                Priority = 1
            }, true).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task StopAsync() {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get base edge configuration
        /// </summary>
        /// <returns></returns>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> CreateLayeredDeployment() {

            var registryCredentials = "";
            if (!string.IsNullOrEmpty(_config.Value.DockerServer) &&
                _config.Value.DockerServer != "mcr.microsoft.com") {
                var registryId = _config.Value.DockerServer.Split('.')[0];
                registryCredentials = @"
                    ""properties.desired.runtime.settings.registryCredentials." + registryId + @""": {
                        ""address"": """ + _config.Value.DockerServer + @""",
                        ""password"": """ + _config.Value.DockerPassword + @""",
                        ""username"": """ + _config.Value.DockerUser + @"""
                    },
                ";
            }

            var server = string.IsNullOrEmpty(_config.Value.DockerServer) ?
                "mcr.microsoft.com" : _config.Value.DockerServer;
            var ns = string.IsNullOrEmpty(_config.Value.ImagesNamespace) ? "" :
                _config.Value.ImagesNamespace.TrimEnd('/') + "/";
            var version = _config.Value.ImagesTag ?? "latest";
            var image = $"{server}/{ns}iotedge/opc-publisher:{version}";

            _logger.LogInformation("Updating opc publisher module deployment with image {image}", image);

            // Return deployment modules object
            var content = @"
            {
                ""$edgeAgent"": {
                    " + registryCredentials + @"
                    ""properties.desired.modules.publisher"": {
                        ""settings"": {
                            ""image"": """ + image + @""",
                            ""createOptions"": {
                            }
                        },
                        ""type"": ""docker"",
                        ""status"": ""running"",
                        ""restartPolicy"": ""always"",
                        ""version"": """ + (version == "latest" ? "1.0" : version) + @"""
                    }
                },
                ""$edgeHub"": {
                    ""properties.desired.routes.upstream"": ""FROM /messages/* INTO $upstream""
                },
                ""publisher"": {
                    ""properties.desired"": {
                        ""LogWorkspaceId"": """ + _diagnostics.Value.LogWorkspaceId + @""",
                        ""LogWorkspaceKey"": """ + _diagnostics.Value.LogWorkspaceKey + @"""
                    }
                }
            }";
            return _serializer
                .Deserialize<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>>(content);
        }

        private const string kDefaultSchemaVersion = "1.0";
        private readonly IJsonSerializer _serializer;
        private readonly IDeviceDeploymentServices _service;
        private readonly IOptions<ContainerRegistryOptions> _config;
        private readonly IOptions<LogAnalyticsOptions> _diagnostics;
        private readonly ILogger _logger;
    }
}
