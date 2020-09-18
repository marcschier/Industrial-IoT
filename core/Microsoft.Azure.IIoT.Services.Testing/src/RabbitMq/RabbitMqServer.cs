﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.RabbitMq.Server {
    using Microsoft.Azure.IIoT.Services.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using global::Docker.DotNet.Models;

    /// <summary>
    /// Represents a rabbit mq server instance
    /// </summary>
    public class RabbitMqServer : DockerContainer, IHostProcess {

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="check"></param>
        /// <param name="ports"></param>
        /// <param name="logger"></param>
        /// <param name="user"></param>
        /// <param name="key"></param>
        public RabbitMqServer(ILogger logger, string user = null, string key = null,
            int[] ports = null, IHealthCheck check = null) : base(logger, null, check) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _user = user;
            _key = key;
            _ports = ports ?? new[] { 5672, 4369, 25672, 15672 }; // TODO
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
                var name = $"rabbitmq_{string.Join("_", _ports)}";
                (_containerId, _owner) = await StartContainerAsync(
                    param, name, "bitnami/rabbitmq:latest");

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_ports.First());
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
                        "RABBITMQ_USERNAME=" + _user,
                        "RABBITMQ_PASSWORD=" + _key,
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
        private readonly ILogger _logger;
        private readonly string _user;
        private readonly string _key;
        private readonly int[] _ports;
        private string _containerId;
        private bool _owner;
    }
}
