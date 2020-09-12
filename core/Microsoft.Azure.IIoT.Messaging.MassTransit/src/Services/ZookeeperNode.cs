// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka.Services {
    using Microsoft.Azure.IIoT.Hosting.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet.Models;

    /// <summary>
    /// Represents a Zookeeper node
    /// </summary>
    public class ZookeeperNode : DockerContainer, IHostProcess {

        /// <summary>
        /// Network name
        /// </summary>
        public string NetworkName { get; set; }

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="port"></param>
        public ZookeeperNode(ILogger logger, int? port = null) : base(logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _port = port ?? 2181;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.Information("Starting Zookeeper node...");
                var param = GetContainerParameters(_port);
                var name = $"zookeeper_{_port}";
                (_containerId, _owner) = await StartContainerAsync(
                    param, name, "bitnami/zookeeper:latest");

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_port);
                    _logger.Information("Zookeeper node running.");
                }
                catch {
                    // Stop and retry
                    await StopContainerAsync(_containerId);
                    _containerId = null;
                    throw;
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync();
            try {
                if (_containerId != null && _owner) {
                    await StopContainerAsync(_containerId);
                    _logger.Information("Stopped Zookeeper node...");
                }
            }
            finally {
                _containerId = null;
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
        }

        /// <summary>
        /// Create create parameters
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private CreateContainerParameters GetContainerParameters(int port) {
            const int zooKeeperPort = 2181;
            return new CreateContainerParameters(
                new Config {
                    ExposedPorts = new Dictionary<string, EmptyStruct>() {
                        [zooKeeperPort.ToString()] = default
                    },
                    Env = new List<string> {
                        "ZOO_ENABLE_AUTH=no",
                        "ALLOW_ANONYMOUS_LOGIN=yes"
                    }
                }) {
                HostConfig = new HostConfig {
                    NetworkMode = NetworkName,
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        [zooKeeperPort.ToString()] = new List<PortBinding> {
                            new PortBinding {
                                HostPort = port.ToString()
                            }
                        }
                    }
                }
            };
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly int _port;
        private string _containerId;
        private bool _owner;
    }
}
