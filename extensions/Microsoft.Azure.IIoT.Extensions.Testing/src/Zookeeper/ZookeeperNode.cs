// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Zookeeper.Server {
    using Microsoft.Azure.IIoT.Extensions.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using global::Docker.DotNet.Models;

    /// <summary>
    /// Represents a Zookeeper node
    /// </summary>
    public class ZookeeperNode : DockerContainer, IHostProcess {

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="check"></param>
        /// <param name="logger"></param>
        /// <param name="networkName"></param>
        /// <param name="port"></param>
        public ZookeeperNode(ILogger logger, string networkName, int? port = null,
            IHealthCheck check = null) : base(logger, networkName, check) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _port = port ?? 2181;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.LogInformation("Starting Zookeeper node...");
                var param = GetContainerParameters(_port);
                var name = $"zookeeper_{_port}";
                (_containerId, _owner) = await CreateAndStartContainerAsync(
                    param, name, "bitnami/zookeeper:latest").ConfigureAwait(false);

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_port).ConfigureAwait(false);
                    _logger.LogInformation("Zookeeper node running.");
                }
                catch {
                    // Stop and retry
                    await StopAndRemoveContainerAsync(_containerId).ConfigureAwait(false);
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
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_containerId != null && _owner) {
                    await StopAndRemoveContainerAsync(_containerId).ConfigureAwait(false);
                    _logger.LogInformation("Stopped Zookeeper node...");
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
