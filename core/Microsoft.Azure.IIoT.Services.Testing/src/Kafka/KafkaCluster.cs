// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.Kafka.Server {
    using Microsoft.Azure.IIoT.Services.Zookeeper.Server;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Represents a Kafka node
    /// </summary>
    public class KafkaCluster : IHostProcess {

        /// <summary>
        /// Create cluster
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="kafkaNodes"></param>
        public KafkaCluster(ILogger logger, int kafkaNodes = 3) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _networkName = "Kafka";
            _zookeeper = new ZookeeperNode(logger, _networkName, 2181);
            _kafkaNodes = kafkaNodes;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_nodes.Count == _kafkaNodes) {
                    return; // Running
                }

                await _zookeeper.StartAsync();

                _logger.Information("Starting Kafka cluster...");
                for (var i = _nodes.Count; i < _kafkaNodes; i++) {
                    var node = new KafkaNode(_logger,
                        $"{_zookeeper.ContainerName}:2181", 9092 + i, _networkName);
                    _nodes.Add(node);
                }
                await Task.WhenAll(_nodes.Select(n => n.StartAsync()));
                _logger.Information("Kafka cluster running.");
            }
            catch {
                await _zookeeper.StopAsync();
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync();
            try {
                if (_nodes.Count == 0) {
                    // Stopped
                    return;
                }
                try {
                    await Task.WhenAll(_nodes.Select(n => n.StopAsync()));
                }
                finally {
                    _nodes.Clear();
                    await _zookeeper.StopAsync();
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly ZookeeperNode _zookeeper;
        private readonly string _networkName;
        private readonly int _kafkaNodes;
        private readonly List<KafkaNode> _nodes = new List<KafkaNode>();
    }
}
