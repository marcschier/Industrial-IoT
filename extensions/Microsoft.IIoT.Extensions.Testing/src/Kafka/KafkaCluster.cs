﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Kafka.Server {
    using Microsoft.IIoT.Extensions.Zookeeper.Server;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Utils;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
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
        /// <param name="checks"></param>
        public KafkaCluster(ILogger logger, IEnumerable<IHealthCheck> checks,
            int kafkaNodes = 3) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _networkName = "Kafka";
            _zookeeper = new ZookeeperNode(logger, _networkName, 2181);
            _kafkaNodes = kafkaNodes;
            _checks = checks?.ToList();
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_nodes.Count == _kafkaNodes) {
                    return; // Running
                }

                await _zookeeper.StartAsync().ConfigureAwait(false);

                _logger.LogInformation("Starting Kafka cluster...");
                for (var i = _nodes.Count; i < _kafkaNodes; i++) {
                    var node = new KafkaNode(_logger,
                        $"{_zookeeper.ContainerName}:2181", 9092 + i, _networkName);
                    _nodes.Add(node);
                }
                await Task.WhenAll(_nodes.Select(n => n.StartAsync())).ConfigureAwait(false);
                await WaitForClusterHealthAsync().ConfigureAwait(false);
                _logger.LogInformation("Kafka cluster running.");
            }
            catch {
                await _zookeeper.StopAsync().ConfigureAwait(false);
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_nodes.Count == 0) {
                    // Stopped
                    return;
                }
                try {
                    await Task.WhenAll(_nodes.Select(n => n.StopAsync())).ConfigureAwait(false);
                }
                finally {
                    _nodes.Clear();
                    await _zookeeper.StopAsync().ConfigureAwait(false);
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

        /// <summary>
        /// Wait for cluster healthiness
        /// </summary>
        /// <returns></returns>
        private async Task WaitForClusterHealthAsync() {
            for (var attempt = 0; attempt < 10; attempt++) {
                var up = true;
                foreach (var check in _checks) {
                    var result = await check.CheckHealthAsync(null).ConfigureAwait(false);
                    if (result.Status != HealthStatus.Healthy) {
                        up = false;
                        break;
                    }
                }
                if (up) {
                    // Up and running
                    return;
                }
                await Task.Delay(1000).ConfigureAwait(false);
            }
            throw new ExternalDependencyException("Cluster not available.");
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly ZookeeperNode _zookeeper;
        private readonly string _networkName;
        private readonly int _kafkaNodes;
        private readonly List<IHealthCheck> _checks;
        private readonly List<KafkaNode> _nodes = new List<KafkaNode>();
    }
}
