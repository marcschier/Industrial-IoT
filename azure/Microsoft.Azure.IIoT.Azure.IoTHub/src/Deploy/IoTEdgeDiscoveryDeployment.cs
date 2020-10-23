// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Deploy {
    using Microsoft.Azure.IIoT.Deploy;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Hub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Azure.LogAnalytics;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Deploys registry module
    /// </summary>
    public sealed class IoTEdgeDiscoveryDeployment : IHostProcess {

        /// <summary>
        /// Create deployer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgeDiscoveryDeployment(IDeviceDeploymentServices service,
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
                Id = "__default-discoverer-linux",
                Content = new ConfigurationContentModel {
                    ModulesContent = CreateLayeredDeployment(true)
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = IoTEdgeBaseDeployment.TargetCondition +
                    " AND tags.os = 'Linux'",
                Priority = 1
            }, true).ConfigureAwait(false);

            await _service.CreateOrUpdateConfigurationAsync(new ConfigurationModel {
                Id = "__default-discoverer-windows",
                Content = new ConfigurationContentModel {
                    ModulesContent = CreateLayeredDeployment(false)
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = IoTEdgeBaseDeployment.TargetCondition +
                    " AND tags.os = 'Windows'",
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
        /// <param name="isLinux"></param>
        /// <returns></returns>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> CreateLayeredDeployment(
            bool isLinux) {

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

            // Configure create options per os specified
            string createOptions;
            if (isLinux) {
                // Linux
                createOptions = _serializer.SerializeToString(new {
                    NetworkingConfig = new {
                        EndpointsConfig = new {
                            host = new {
                            }
                        }
                    },
                    HostConfig = new {
                        NetworkMode = "host",
                        CapAdd = new[] { "NET_ADMIN" }
                    },
                    Hostname = "discovery"
                });
            }
            else {
                // Windows
                createOptions = "{}";
            }
            createOptions = createOptions.Replace("\"", "\\\"", StringComparison.Ordinal);

            var server = string.IsNullOrEmpty(_config.DockerServer) ?
                "mcr.microsoft.com" : _config.DockerServer;
            var ns = string.IsNullOrEmpty(_config.ImagesNamespace) ?
                "" : _config.ImagesNamespace.TrimEnd('/') + "/";
            var version = _config.ImagesTag ?? "latest";
            var image = $"{server}/{ns}iotedge/discovery:{version}";

            _logger.LogInformation("Updating discovery module deployment with image {image} for {os}",
                image, isLinux ? "Linux" : "Windows");

            // Return deployment modules object
            var content = @"
            {
                ""$edgeAgent"": {
                    " + registryCredentials + @"
                    ""properties.desired.modules.discovery"": {
                        ""settings"": {
                            ""image"": """ + image + @""",
                            ""createOptions"": """ + createOptions + @"""
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
                ""discovery"": {
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
        private readonly ILogAnalyticsConfig _diagnostics;
        private readonly IContainerRegistryConfig _config;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
    }
}
