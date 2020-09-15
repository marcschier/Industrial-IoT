﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using Serilog;

    /// <summary>
    /// Worker host
    /// </summary>
    public abstract class HostProcess : IHostProcess, IDisposable {

        /// <summary>
        /// Name of the process
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        public HostProcess(ILogger logger, string name = null) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lock = new SemaphoreSlim(1, 1);
            Name = string.IsNullOrEmpty(name) ? "Worker" : name;
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync();
            try {
                if (_host != null) {
                    _logger.Debug("{host} host already running.", Name);
                    return;
                }
                _logger.Debug("Starting {host} host...", Name);
                _host = new Worker(ct => RunAsync(ct));
                _logger.Information("{host} host started.", Name);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Error starting {host} host.", Name);
                _host = null;
                throw ex;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync();
            try {
                if (_host == null) {
                    return;
                }
                try {
                    _logger.Debug("Stopping {host} host...", Name);
                    await _host.DisposeAsync();
                    _logger.Information("{host} host stopped.", Name);
                }
                finally {
                    _host = null;
                }
            }
            catch (Exception ex) {
                _logger.Warning(ex, "Error stopping {host} host", Name);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
            _lock.Dispose();
        }

        /// <summary>
        /// Processing loop
        /// </summary>
        /// <param name="ct"></param>
        protected abstract Task RunAsync(CancellationToken ct);

        private Worker _host;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _lock;
    }
}
