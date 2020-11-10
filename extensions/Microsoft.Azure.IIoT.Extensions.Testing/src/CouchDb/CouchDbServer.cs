﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.CouchDb.Server {
    using Microsoft.Azure.IIoT.Extensions.Docker;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using global::Docker.DotNet.Models;

    /// <summary>
    /// Represents a CouchDB node
    /// </summary>
    public class CouchDbServer : DockerContainer, IHostProcess {

        /// <summary>
        /// Create node
        /// </summary>
        /// <param name="check"></param>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        /// <param name="user"></param>
        /// <param name="key"></param>
        public CouchDbServer(ILogger logger, string user = null, string key = null,
            int? port = null, IHealthCheck check = null) : base(logger, null, check) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _user = user;
            _key = key;
            _port = port ?? 5984;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_containerId != null) {
                    return;
                }

                _logger.LogInformation("Starting CouchDB server at {port}...", _port);
                var param = GetContainerParameters(_port);
                var name = $"couchdb_{_port}";
                (_containerId, _owner) = await CreateAndStartContainerAsync(
                    param, name, "bitnami/couchdb:latest").ConfigureAwait(false);

                try {
                    // Check running
                    await WaitForContainerStartedAsync(_port).ConfigureAwait(false);
                    _logger.LogInformation("CouchDB server running at {port}.", _port);
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
                    _logger.LogInformation("Stopped CouchDB server at {port}.", _port);
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
                        "COUCHDB_USER=" + _user ?? "admin",
                        "COUCHDB_PASSWORD=" + _key ?? "couchdb",
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
        private readonly string _user;
        private readonly string _key;
        private readonly int _port;
        private string _containerId;
        private bool _owner;
    }
}