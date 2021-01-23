// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Testing.Fixtures {
    using Microsoft.IIoT.Protocols.OpcUa.Testing.Runtime;
    using Microsoft.IIoT.Protocols.OpcUa.Services;
    using Microsoft.IIoT.Protocols.OpcUa.Sample;
    using Microsoft.IIoT.Protocols.OpcUa;
    using Microsoft.IIoT.Extensions.Diagnostics;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.Extensions.Logging;
    using Opc.Ua.Server;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    /// <summary>
    /// Adds sample server as fixture to unit tests
    /// </summary>
    public abstract class BaseServerFixture : IDisposable {

        /// <summary>
        /// Port server is listening on
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Certificate of the server
        /// </summary>
        public X509Certificate2 Certificate => _serverHost.Certificate;

        /// <summary>
        /// Cert folder
        /// </summary>
        public string PkiRootPath { get; private set; }

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Client
        /// </summary>
        public ClientServices Client => _client.Value;

        /// <summary>
        /// Start port
        /// </summary>
        public static void SetStartPort(int value) {
            _nextPort = value;
        }

        /// <summary>
        /// Create fixture
        /// </summary>
        protected BaseServerFixture(IEnumerable<INodeManagerFactory> nodes) {
            if (nodes == null) {
                throw new ArgumentNullException(nameof(nodes));
            }
            Logger = Log.Console(LogLevel.Debug);
            _config = new TestClientServicesConfig();
            _client = new Lazy<ClientServices>(() => {
                return new ClientServices(Logger, _config);
            }, false);
            PkiRootPath = Path.Combine(Directory.GetCurrentDirectory(), "pki",
               Guid.NewGuid().ToByteArray().ToBase16String());
            var port = Interlocked.Increment(ref _nextPort);
            for (var i = 0; i < 200; i++) { // Retry 200 times
                try {
                    _serverHost = new ServerConsoleHost(
                        new ServerFactory(Logger, nodes) {
                            LogStatus = false
                        }, Logger) {
                        PkiRootPath = PkiRootPath,
                        AutoAccept = true
                    };
                    Logger.LogInformation("Starting server host on {port}...",
                        port);
                    _serverHost.StartAsync(new int[] { port }).Wait();
                    Port = port;
                    break;
                }
                catch (Exception ex) {
                    port = Interlocked.Increment(ref _nextPort);
                    Logger.LogError(ex, "Failed to start server host, retrying {port}...",
                        port);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override to dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    Logger.LogInformation("Disposing server and client fixture...");
                    _serverHost.Dispose();
                    // Clean up all created certificates
                    if (Directory.Exists(PkiRootPath)) {
                        Logger.LogInformation("Server disposed - cleaning up server certificates...");
                        Try.Op(() => Directory.Delete(PkiRootPath, true));
                    }
                    if (_client.IsValueCreated) {
                        Logger.LogInformation("Disposing client...");
                        Task.Run(() => _client.Value.Dispose()).Wait();
                    }
                    Logger.LogInformation("Client disposed - cleaning up client certificates...");
                    _config?.Dispose();
                    Logger.LogInformation("Server and client fixture disposed.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        private static readonly Random kRand = new Random();
        private static volatile int _nextPort = kRand.Next(53000, 58000);
        private bool _disposedValue;
        private readonly IServerHost _serverHost;
        private readonly TestClientServicesConfig _config;
        private readonly Lazy<ClientServices> _client;
    }
}
