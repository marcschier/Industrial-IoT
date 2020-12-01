// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Deploy {
    using Microsoft.IIoT.Azure.LogAnalytics;
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Azure.IoTHub.Models;
    using Microsoft.IIoT.Serializers;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Deploys metricscollector module
    /// </summary>
    public sealed class IoTEdgeMetricsDeployment : IHostProcess {

        /// <summary>
        /// Create deployer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public IoTEdgeMetricsDeployment(IDeviceDeploymentServices service,
            IOptions<LogAnalyticsOptions> config, IJsonSerializer serializer, ILogger logger) {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _config = config ?? throw new ArgumentNullException(nameof(service));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            if (string.IsNullOrEmpty(_config.Value.LogWorkspaceId) ||
                string.IsNullOrEmpty(_config.Value.LogWorkspaceKey)) {
                _logger.LogWarning("Azure Log Analytics Workspace configuration is not set." +
                    " Cannot proceed with metricscollector deployment.");
                return;
            }
            await _service.CreateOrUpdateConfigurationAsync(new ConfigurationModel {
                Id = "__default-metricscollector-linux",
                Content = new ConfigurationContentModel {
                    ModulesContent = CreateLayeredDeployment(true)
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = IoTEdgeBaseDeployment.TargetCondition +
                    " AND tags.os = 'Linux'",
                Priority = 2
            }, true).ConfigureAwait(false);

            await _service.CreateOrUpdateConfigurationAsync(new ConfigurationModel {
                Id = "__default-metricscollector-windows",
                Content = new ConfigurationContentModel {
                    ModulesContent = CreateLayeredDeployment(false)
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = IoTEdgeBaseDeployment.TargetCondition +
                    " AND tags.os = 'Windows'",
                Priority = 2
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

            // Configure create options and version per os specified
            string createOptions;
            if (isLinux) {
                // Linux
                createOptions = "{}";
            }
            else {
                // Windows
                createOptions = _serializer.SerializeToString(new {
                    User = "ContainerAdministrator"
                });
            }
            createOptions = createOptions.Replace("\"", "\\\"", StringComparison.Ordinal);
            var image = $"azureiotedge/azureiotedge-metrics-collector-sample:0.1";
            _logger.LogInformation("Updating metrics collector module deployment for {os}", isLinux ? "Linux" : "Windows");

            // Return deployment modules object
            var content = @"
            {
                ""$edgeAgent"": {
                    ""properties.desired.modules.metricscollector"": {
                        ""settings"": {
                            ""image"": """ + image + @""",
                            ""createOptions"": """ + createOptions + @"""
                        },
                        ""type"": ""docker"",
                        ""version"": ""1.0"",
                        ""env"": {
                            ""LogAnalyticsWorkspaceId"": {
                                ""value"": """ + _config.Value.LogWorkspaceId + @"""
                            },
                            ""LogAnalyticsSharedKey"": {
                                ""value"": """ + _config.Value.LogWorkspaceKey + @"""
                            },
                            ""LogAnalyticsLogType"": {
                                ""value"": ""promMetrics""
                            },
                            ""MetricsEndpointsCSV"": {
                                ""value"": ""http://twin:9701/metrics,http://publisher:9702/metrics""
                            },
                            ""ScrapeFrequencyInSecs"": {
                                ""value"": ""120""
                            },
                            ""UploadTarget"": {
                                ""value"": ""AzureLogAnalytics""
                            }
                        },
                        ""status"": ""running"",
                        ""restartPolicy"": ""always""
                    }
                },
                ""$edgeHub"": {
                    ""properties.desired.routes.upstream"": ""FROM /messages/* INTO $upstream""
                }
            }";
            return _serializer
                .Deserialize<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>>(content);
        }

        private const string kDefaultSchemaVersion = "1.0";
        private readonly IDeviceDeploymentServices _service;
        private readonly IOptions<LogAnalyticsOptions> _config;
        private readonly IJsonSerializer _serializer;
        private readonly ILogger _logger;
    }
}
