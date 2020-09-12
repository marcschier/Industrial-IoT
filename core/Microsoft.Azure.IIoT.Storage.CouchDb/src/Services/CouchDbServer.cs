// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Storage.CouchDb.Services {
    using Microsoft.Azure.IIoT.Hosting.Docker;
    using Microsoft.Azure.IIoT.Utils;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet.Models;

    /// <summary>
    /// Represents a CouchDB node
    /// </summary>
    public class CouchDbServer : DockerContainer, IHostProcess {

        /// <summary>
        /// Network name
        /// </summary>
        public string NetworkName { get; set; }

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        public CouchDbServer(ILogger logger, int? port = null) : base(logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _port = port ?? 5984;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.Information("Starting CouchDB server at {port}...", _port);
                var param = GetContainerParameters(_port);
                var name = $"couchdb_{_port}";
                (_containerId, _owner) = await StartContainerAsync(
                    param, name, "bitnami/couchdb:latest");

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_port);
                    _logger.Information("CouchDB server running at {port}.", _port);
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
                    _logger.Information("Stopped CouchDB server at {port}.", _port);
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
            const int couchPort = 5984;
            return new CreateContainerParameters(
                new Config {
                    ExposedPorts = new Dictionary<string, EmptyStruct>() {
                        [couchPort.ToString()] = default
                    },
                    Env = new List<string> {
                        "COUCHDB_CREATE_DATABASES=yes",
                        "COUCHDB_USER=admin",
                        "COUCHDB_PASSWORD=couchdb",
                    }
                }) {
                HostConfig = new HostConfig {
                    NetworkMode = NetworkName,
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        [couchPort.ToString()] = new List<PortBinding> {
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
