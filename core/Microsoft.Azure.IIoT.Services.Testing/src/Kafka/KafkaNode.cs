// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Server {
    using Microsoft.Azure.IIoT.Services.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;
    using global::Docker.DotNet.Models;

    /// <summary>
    /// Represents a Kafka node
    /// </summary>
    public class KafkaNode : DockerContainer, IHostProcess {

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="port"></param>
        /// <param name="zookeeper"></param>
        /// <param name="networkName"></param>
        /// <param name="check"></param>
        /// <param name="logger"></param>
        public KafkaNode(ILogger logger, string zookeeper, int port, string networkName,
            IHealthCheck check = null) : base(logger, networkName, check) {
            _zookeeper = zookeeper ?? throw new ArgumentNullException(nameof(zookeeper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _port = port;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.Information("Starting Kafka node at {port}...", _port);
                var param = GetContainerParameters(_port);
                var name = $"kafka_{_port}";
                (_containerId, _owner) = await CreateAndStartContainerAsync(
                    param, name, "bitnami/kafka:latest").ConfigureAwait(false);

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_port).ConfigureAwait(false);
                    _logger.Information("Kafka node running at {port}.", _port);
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
                    _logger.Information("Stopped Kafka node at {port}.", _port);
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
            const int kafkaPort = 9092;
            return new CreateContainerParameters(
                new Config {
                    ExposedPorts = new Dictionary<string, EmptyStruct>() {
                        [kafkaPort.ToString()] = default
                    },
                    Env = new List<string> {
                        $"KAFKA_CFG_ZOOKEEPER_CONNECT={_zookeeper}",
                        "ALLOW_PLAINTEXT_LISTENER=yes",
                        "KAFKA_CFG_LISTENERS=PLAINTEXT://0.0.0.0:"+ kafkaPort,
                        "KAFKA_CFG_ADVERTISED_LISTENERS=PLAINTEXT://" + HostIp.Value + ":" + port,
                    }
                }) {
                HostConfig = new HostConfig {
                    NetworkMode = NetworkName,
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        [kafkaPort.ToString()] = new List<PortBinding> {
                            new PortBinding {
                                HostPort = port.ToString()
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Gets ip address
        /// </summary>
        private static Lazy<string> HostIp { get; } = new Lazy<string>(() => {
            try {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch {
                return Dns.GetHostAddresses(Dns.GetHostName())
                    .First(i => i.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
        });

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly string _zookeeper;
        private readonly int _port;
        private string _containerId;
        private bool _owner;
    }
}
