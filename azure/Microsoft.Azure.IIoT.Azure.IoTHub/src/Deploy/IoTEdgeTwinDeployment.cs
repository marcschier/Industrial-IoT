// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Deploy {
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Azure.IIoT.Deploy;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Deploys twin module
    /// </summary>
    public sealed class IoTEdgeTwinDeployment : IHostProcess {

        /// <summary>
        /// Create deployer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgeTwinDeployment(IDeviceDeploymentServices service,
            IContainerRegistryConfig config, ILogAnalyticsConfig diagnostics,
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
                Id = "__default-opctwin",
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
            if (!string.IsNullOrEmpty(_config.DockerServer) &&
                _config.DockerServer != "mcr.microsoft.com") {
                var registryId = _config.DockerServer.Split('.')[0];
                registryCredentials = @"
                    ""properties.desired.runtime.settings.registryCredentials." + registryId + @""": {
                        ""address"": """ + _config.DockerServer + @""",
                        ""password"": """ + _config.DockerPassword + @""",
                        ""username"": """ + _config.DockerUser + @"""
                    },
                ";
            }

            var server = string.IsNullOrEmpty(_config.DockerServer) ?
                "mcr.microsoft.com" : _config.DockerServer;
            var ns = string.IsNullOrEmpty(_config.ImagesNamespace) ? "" :
                _config.ImagesNamespace.TrimEnd('/') + "/";
            var version = _config.ImagesTag ?? "latest";
            var image = $"{server}/{ns}iotedge/opc-twin:{version}";

            _logger.Information("Updating opc twin module deployment with image {image}", image);

            // Return deployment modules object
            var content = @"
            {
                ""$edgeAgent"": {
                    " + registryCredentials + @"
                    ""properties.desired.modules.twin"": {
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
                ""twin"": {
                    ""properties.desired"": {
                        ""LogWorkspaceId"": """ + _diagnostics.LogWorkspaceId + @""",
                        ""LogWorkspaceKey"": """ + _diagnostics.LogWorkspaceKey + @"""
                    }
                }
            }";
            return _serializer
                .Deserialize<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>>(content);
        }

        private const string kDefaultSchemaVersion = "1.0";
        private readonly IDeviceDeploymentServices _service;
        private readonly IContainerRegistryConfig _config;
        private readonly ILogAnalyticsConfig _diagnostics;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
    }
}
