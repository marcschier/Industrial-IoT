// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.RabbitMq.Services {
    using Microsoft.Azure.IIoT.Messaging.RabbitMq;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet.Models;

    /// <summary>
    /// Represents a rabbit mq server instance
    /// </summary>
    public class RabbitMqServer : DockerContainer, IHostProcess {

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public RabbitMqServer(IRabbitMqConfig config, ILogger logger) : base(logger) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ports = new[] { 5672, 4369, 25672, 15672 }; // TODO
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.Information("Starting RabbitMq server...");
                var param = GetContainerParameters(_ports);
                var name = $"rabbitmq_{string.Join("_", _ports)}_{param.GetHashCode()}";
                (_containerId, _owner) = await StartContainerAsync(
                    param, name, "bitnami/rabbitmq:latest");

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_ports);
                    _logger.Information("RabbitMq server running.");
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
                    _logger.Information("Stopped RabbitMq server...");
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
        /// <param name="hostPorts"></param>
        /// <returns></returns>
        private CreateContainerParameters GetContainerParameters(int[] hostPorts) {
            var containerPorts = new[] { 5672, 4369, 25672, 15672 };
            return new CreateContainerParameters(
                new Config {
                    ExposedPorts = containerPorts
                        .ToDictionary<int, string, EmptyStruct>(p => p.ToString(), _ => default),
                    Env = new List<string>(new[] {
                        "RABBITMQ_USERNAME=" + _config.UserName,
                        "RABBITMQ_PASSWORD=" + _config.Key,
                    })
                }) {
                HostConfig = new HostConfig {
                    PortBindings = containerPorts.ToDictionary(k => k.ToString(),
                    v => (IList<PortBinding>)new List<PortBinding> {
                        new PortBinding {
                            HostPort = hostPorts[Array.IndexOf(containerPorts, v) %
                            hostPorts.Length].ToString()
                        }
                    })
                }
            };
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IRabbitMqConfig _config;
        private readonly ILogger _logger;
        private readonly int[] _ports;
        private string _containerId;
        private bool _owner;
    }
}
