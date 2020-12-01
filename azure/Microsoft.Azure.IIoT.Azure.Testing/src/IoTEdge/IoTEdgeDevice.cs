// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Testing {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Azure.IoTHub.Models;
    using Microsoft.Azure.IIoT.Extensions.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet.Models;

    /// <summary>
    /// Represents an IoT Edge device containerized
    /// </summary>
    public class IoTEdgeDevice : DockerContainer, IHostProcess {

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="check"></param>
        /// <param name="ports"></param>
        /// <param name="logger"></param>
        /// <param name="deviceId"></param>
        /// <param name="services"></param>
        public IoTEdgeDevice(IDeviceTwinServices services, string deviceId,
            ILogger logger, int[] ports = null, IHealthCheck check = null) :
            this(GetConnectionStringAsync(services, deviceId, logger).Result,
                logger, ports, check) {
        }

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="check"></param>
        /// <param name="ports"></param>
        /// <param name="logger"></param>
        /// <param name="cs"></param>
        public IoTEdgeDevice(ConnectionString cs, ILogger logger, int[] ports = null,
            IHealthCheck check = null) : base(logger, null, check) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cs = cs ?? throw new ArgumentNullException(nameof(cs));
            if (ports == null || ports.Length == 0) {
                ports = new[] { 15580, 15581, 1883, 8883, 5276, 443 }; // TODO
            }
            _ports = ports;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.LogInformation("Starting IoTEdge device...");
                var param = GetContainerParameters(_ports);
                var name = $"iotedge_{string.Join("_", _ports)}";
                (_containerId, _owner) = await CreateAndStartContainerAsync(
                    param, name, "marcschier/iotedge:latest").ConfigureAwait(false);

                try {
                    // Check running
                    await WaitForContainerStartedAsync(
                        _ports.First()).ConfigureAwait(false);
                    _logger.LogInformation("IoTEdge device running.");
                }
                catch {
                    // Stop and retry
                    await StopAndRemoveContainerAsync(
                        _containerId).ConfigureAwait(false);
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
                    await StopAndRemoveContainerAsync(
                        _containerId).ConfigureAwait(false);
                    _logger.LogInformation("Stopped IoTEdge device.");
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
            var containerPorts = new[] { 15580, 15581, 1883, 8883, 5276, 443 };
            return new CreateContainerParameters(
                new Config {
                    Hostname = _cs.DeviceId,
                    ExposedPorts = containerPorts
                        .ToDictionary<int, string, EmptyStruct>(p => p.ToString(), _ => default),
                    Env = new List<string> {
                        "connectionString=" + _cs.ToString()
                    }
                }) {
                Name = _cs.DeviceId,
                HostConfig = new HostConfig {
                    Privileged = true,
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

        /// <summary>
        /// Create device if it does not exist and return connection string.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="deviceId"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static async Task<ConnectionString> GetConnectionStringAsync(
            IDeviceTwinServices registry, string deviceId, ILogger logger) {
            try {
                await registry.RegisterAsync(new DeviceRegistrationModel {
                    Id = deviceId,
                    Tags = new Dictionary<string, VariantValue> {
                        [TwinProperty.Type] = "iiotedge"
                    },
                    Capabilities = new DeviceCapabilitiesModel {
                        IotEdge = true
                    }
                }, false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ResourceConflictException) {
                logger.LogInformation("Gateway {deviceId} exists.", deviceId);
            }
            return await registry.GetConnectionStringAsync(
                deviceId).ConfigureAwait(false);
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly ConnectionString _cs;
        private readonly int[] _ports;
        private string _containerId;
        private bool _owner;
    }
}
