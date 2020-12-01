// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Deploy {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Default edge base deployment configuration
    /// </summary>
    public sealed class IoTEdgeBaseDeployment : IHostProcess {

        /// <summary>
        /// Target condition for gateways
        /// </summary>
        public static readonly string TargetCondition =
            $"(tags.__type__ = '{kEdgeType}' AND NOT IS_DEFINED(tags.unmanaged))";

        /// <summary>
        /// Create edge base deployer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="serializer"></param>
        public IoTEdgeBaseDeployment(IDeviceDeploymentServices service,
            IJsonSerializer serializer) {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public Task StartAsync() {
           return _service.CreateOrUpdateConfigurationAsync(new ConfigurationModel {
                Id = kEdgeType,
                Content = new ConfigurationContentModel {
                    ModulesContent = GetEdgeBase()
                },
                SchemaVersion = kDefaultSchemaVersion,
                TargetCondition = TargetCondition,
                Priority = 0
            }, true);
        }

        /// <inheritdoc/>
        public Task StopAsync() {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get base edge configuration
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> GetEdgeBase(
            string version = "1.2.0-rc1") {
            return _serializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>>(@"
{
    ""$edgeAgent"": {
        ""properties.desired"": {
            ""schemaVersion"": """ + kDefaultSchemaVersion + @""",
            ""runtime"": {
                ""type"": ""docker"",
                ""settings"": {
                    ""minDockerVersion"": ""v1.25"",
                    ""loggingOptions"": """",
                    ""registryCredentials"": {
                    }
                }
            },
            ""systemModules"": {
                ""edgeAgent"": {
                    ""type"": ""docker"",
                    ""settings"": {
                        ""image"": ""mcr.microsoft.com/azureiotedge-agent:" + version+ @""",
                        ""createOptions"": ""{}""
                    },
                    ""env"": {
                        ""experimentalFeatures__enabled"": {
                            ""value"": ""true""
                        },
                        ""experimentalFeatures__enableGetLogs"": {
                            ""value"": ""true""
                        },
                        ""experimentalFeatures__enableUploadLogs"": {
                            ""value"": ""true""
                        },
                        ""experimentalFeatures__enableMetrics"": {
                            ""value"": ""true""
                        }
                    }
                },
                ""edgeHub"": {
                    ""type"": ""docker"",
                    ""status"": ""running"",
                    ""restartPolicy"": ""always"",
                    ""settings"": {
                        ""image"": ""mcr.microsoft.com/azureiotedge-hub:" + version + @""",
                        ""createOptions"": ""{\""HostConfig\"":{\""PortBindings\"":{\""443/tcp\"":[{\""HostPort\"":\""443\""}],\""5671/tcp\"":[{\""HostPort\"":\""5671\""}],\""8883/tcp\"":[{\""HostPort\"":\""8883\""}],\""1883/tcp\"":[{\""HostPort\"":\""1883\""}]}}}""
                    },
                    ""env"": {
                        ""experimentalFeatures__enabled"": {
                            ""value"": ""true""
                        },
                        ""experimentalFeatures__mqttBrokerEnabled"": {
                            ""value"": ""true""
                        },
                        ""experimentalFeatures__nestedEdgeEnabled"": {
                            ""value"": ""true""
                        }
                    }
                }
            },
            ""modules"": {
                ""registry"": {
                    ""settings"": {
                        ""image"": ""registry:latest"",
                        ""createOptions"": ""{\""HostConfig\"":{\""PortBindings\"":{\""5000/tcp\"":[{\""HostPort\"":\""5000\""}]}}}""
                    },
                    ""type"": ""docker"",
                    ""version"": ""1.0"",
                    ""env"": {
                        ""REGISTRY_PROXY_REMOTEURL"": {
                            ""value"": ""https://mcr.microsoft.com""
                        }
                    },
                    ""status"": ""running"",
                    ""restartPolicy"": ""always""
                },
                ""IoTEdgeAPIProxy"": {
                    ""settings"": {
                        ""image"": ""mcr.microsoft.com/azureiotedge-api-proxy"",
                        ""createOptions"": ""{\""HostConfig\"": {\""PortBindings\"": {\""8000/tcp\"": [{\""HostPort\"":\""8000\""}]}}}""
                    },
                    ""type"": ""docker"",
                    ""version"": ""1.0"",
                    ""env"": {
                        ""NGINX_DEFAULT_PORT"": {
                            ""value"": ""8000""
                        },
                        ""DOCKER_REQUEST_ROUTE_ADDRESS"": {
                            ""value"": ""registry:5000""
                        },
                        ""BLOB_UPLOAD_ROUTE_ADDRESS"": {
                            ""value"": ""AzureBlobStorageonIoTEdge:11002""
                        }
                    },
                    ""status"": ""running"",
                    ""restartPolicy"": ""always""
                }
            }
        }
    },
    ""$edgeHub"": {
        ""properties.desired"": {
            ""schemaVersion"": ""1.2"",
            ""routes"": {
                ""upstream"": ""FROM /messages/* INTO $upstream""
            },
            ""mqttBroker"": {
                ""authorizations"": [
                    {
                        ""identities"": [
                            ""{{iot:identity}}""
                        ],
                        ""allow"": [
                            {
                                ""operations"": [
                                    ""mqtt:connect"",
                                    ""mqtt:subscribe"",
                                    ""mqtt:publish""
                                ],
                                ""resources"": [
                                    ""#""
                                ]
                            }
                        ]
                    }
                ]
            },
            ""storeAndForwardConfiguration"": {
                ""timeToLiveSecs"": 7200
            }
        }
    }
}
");
        }

        private const string kEdgeType = "iiotedge";
        private const string kDefaultSchemaVersion = "1.1";
        private readonly IDeviceDeploymentServices _service;
        private readonly IJsonSerializer _serializer;
    }
}
