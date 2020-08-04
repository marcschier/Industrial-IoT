// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Hosting {
    using Microsoft.Azure.IIoT.Azure.IoTEdge;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Messaging;
    using Autofac;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity manager manages host processes
    /// </summary>
    public class IoTEdgeHostManager : IDisposable, IModuleHostManager {

        /// <inheritdoc/>
        public IEnumerable<(string, bool)> Hosts {
            get {
                _lock.Wait();
                try {
                    return _hosts
                        .Select(h => (h.Key, h.Value.Connected))
                        .ToList();
                }
                finally {
                    _lock.Release();
                }
            }
        }

        /// <summary>
        /// Create supervisor
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public IoTEdgeHostManager(IContainerFactory factory, IIoTEdgeConfig config,
            ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        public async Task StartAsync(string id, string secret, CancellationToken ct) {
            await _lock.WaitAsync();
            try {
                if (_hosts.TryGetValue(id, out var host) && host.Running) {
                    _logger.Debug("{id} host already running.", id);
                    return;
                }
                _logger.Debug("{id} host starting...", id);
                _hosts.Remove(id);
                host = new IdentityHostProcess(this, _config, id, secret, _logger);
                _hosts.Add(id, host);

                var hosts = _hosts.Count;
                ThreadPool.GetMinThreads(out var workerThreads, out var asyncThreads);
                if (hosts > workerThreads || hosts > asyncThreads) {
                    var result = ThreadPool.SetMinThreads(hosts, hosts);
                    _logger.Information("Thread pool changed to support {async} threads {success}",
                        hosts, result ? "succeeded" : "failed");
                }

                //
                // This starts and waits for the host to be started - versus attaching which
                // represents the state of the actived and supervised hosts in the supervisor
                // device host.
                //
                await host.Started;
                _logger.Information("{id} host started.", id);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(string id, CancellationToken ct) {
            IdentityHostProcess host;
            await _lock.WaitAsync();
            try {
                if (!_hosts.TryGetValue(id, out host)) {
                    _logger.Debug("{id} entity host not running.", id);
                    return;
                }
                _hosts.Remove(id);
            }
            finally {
                _lock.Release();
            }
            //
            // This stops and waits for the host to be stopped - versus detaching which
            // represents the state of the supervised hosts through the supervisor device
            // host.
            //
            await StopOneTwinAsync(id, host);
        }

        /// <inheritdoc/>
        public async Task QueueStartAsync(string id, string secret) {
            await _lock.WaitAsync();
            try {
                if (_hosts.TryGetValue(id, out var host)) {
                    _logger.Debug("{id} host already attached.", id);
                    return;
                }
                _logger.Debug("Attaching entity {id} host...", id);
                host = new IdentityHostProcess(this, _config, id, secret, _logger);
                _hosts.Add(id, host);

                _logger.Information("{id} host attached to module.", id);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task QueueStopAsync(string id) {
            IdentityHostProcess host;
            await _lock.WaitAsync();
            try {
                if (!_hosts.TryGetValue(id, out host)) {
                    _logger.Debug("{id} host not attached.", id);
                    return;
                }

                // Test whether host is in error state and only then remove
                if (!host.Running) {
                    // Detach host that is not running anymore
                    _hosts.Remove(id);
                }
                _logger.Information("{id} host detached from module.", id);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            try {
                StopAllHostsAsync().Wait();
                _logger.Information("All hosts stopped - supervisor exiting...");
                _lock.Dispose();
            }
            catch (Exception e) {
                _logger.Error(e, "Failure exiting supervisor.");
            }
        }

        /// <summary>
        /// Stop one host
        /// </summary>
        /// <param name="id"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        private async Task StopOneTwinAsync(string id, IdentityHostProcess host) {
            _logger.Debug("{id} host is stopping...", id);
            try {
                // Stop host async
                await host.StopAsync();
            }
            catch (Exception ex) {
                // BUGBUG: IoT Hub client SDK throws general exceptions independent
                // of what actually happened.  Instead of parsing the message,
                // just continue.
                _logger.Debug(ex,
                    "{id} host stopping raised exception, continue...", id);
            }
            finally {
                host.Dispose();
            }
            _logger.Information("{id} host stopped.", id);
        }

        /// <summary>
        /// Stop all hosts
        /// </summary>
        /// <returns></returns>
        private async Task StopAllHostsAsync() {
            IList<KeyValuePair<string, IdentityHostProcess>> hosts;
            try {
                await _lock.WaitAsync();
                hosts = _hosts.ToList();
                _hosts.Clear();
            }
            finally {
                _lock.Release();
            }
            await Task.WhenAll(hosts
                .Select(kv => StopOneTwinAsync(kv.Key, kv.Value))
                .ToArray());
        }

        /// <summary>
        /// Runs a device client connected to transparent gateway
        /// </summary>
        private class IdentityHostProcess : IDisposable, IProcessControl, IIoTEdgeConfig {

            /// <summary>
            /// Whether the host is running
            /// </summary>
            public bool Running => !(_runner?.IsCompleted ?? true);

            /// <summary>
            /// Activation state
            /// </summary>
            public bool Connected { get; private set; }

            /// <summary>
            /// Wait until running
            /// </summary>
            public Task Started => _started.Task;

            /// <inheritdoc/>
            public string EdgeHubConnectionString { get; }
            /// <inheritdoc/>
            public bool BypassCertVerification { get; }
            /// <inheritdoc/>
            public TransportOption Transport { get; }

            /// <summary>
            /// Create runner
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="config"></param>
            /// <param name="deviceId"></param>
            /// <param name="secret"></param>
            /// <param name="logger"></param>
            public IdentityHostProcess(IoTEdgeHostManager outer, IIoTEdgeConfig config,
                string deviceId, string secret, ILogger logger) {
                _outer = outer;
                _logger = (logger ?? Log.Logger)
                     .ForContext("SourceContext", deviceId, true);

                BypassCertVerification = config.BypassCertVerification;
                Transport = config.Transport;
                EdgeHubConnectionString = GetEdgeHubConnectionString(config,
                    deviceId, secret);

                // Create host scoped component context for the host
                _container = outer._factory.Create(builder => {
                    builder.RegisterInstance(this)
                        .AsImplementedInterfaces();
                });

                _cts = new CancellationTokenSource();
                _reset = new TaskCompletionSource<bool>();
                _started = new TaskCompletionSource<bool>();
                Connected = false;
                _runner = Task.Run(RunAsync);
            }

            /// <inheritdoc/>
            public void Dispose() {
                StopAsync().Wait();
                _cts.Dispose();
            }

            /// <inheritdoc/>
            public void Reset() {
                _reset?.TrySetResult(true);
            }

            /// <inheritdoc/>
            public void Exit(int exitCode) {
                _cts.Cancel();
            }

            /// <summary>
            /// Shutdown host
            /// </summary>
            /// <returns></returns>
            public async Task StopAsync() {
                if (_container != null) {
                    try {
                        _logger.Information("Initiating identity host exit...");
                        // Cancel runner
                        _cts.Cancel();
                        await _runner;
                    }
                    catch (OperationCanceledException) { }
                    finally {
                        _container.Dispose();
                        _container = null;
                    }
                }
            }

            /// <summary>
            /// Run module host for the identity
            /// </summary>
            /// <returns></returns>
            private async Task RunAsync() {
                var host = _container.Resolve<IModuleHost>();

                var retryCount = 0;
                var cancel = new TaskCompletionSource<bool>();
                _cts.Token.Register(() => cancel.TrySetResult(true));
                _logger.Information("Starting identity host...");
                while (!_cts.Token.IsCancellationRequested) {
                    // Wait until the module unloads or is cancelled
                    try {
                        var version = GetType().Assembly.GetReleaseVersion().ToString();
                        await host.StartAsync("host", "OpcTwin", version, this);
                        Connected = true;
                        _started.TrySetResult(true);
                        _logger.Debug("Twin host (re-)started.");
                        // Reset retry counter on success
                        retryCount = 0;
                        await Task.WhenAny(cancel.Task, _reset.Task);
                        _reset = new TaskCompletionSource<bool>();
                        _logger.Debug("Twin reset requested...");
                    }
                    catch (Exception ex) {
                        Connected = false;

                        var notFound = ex.GetFirstOf<ResourceNotFoundException>();
                        if (notFound != null) {
                            _logger.Information(notFound,
                                "Twin was deleted - exit host...");
                            _started.TrySetException(notFound);
                            return;
                        }
                        var auth = ex.GetFirstOf<ResourceUnauthorizedException>();
                        if (auth != null) {
                            _logger.Information(auth,
                                "Twin not authorized using given secret - exit host...");
                            _started.TrySetException(auth);
                            return;
                        }

                        // Linearly delay on exception since we get these when
                        // the host was deleted.
                        await Try.Async(() =>
                            Task.Delay(kRetryDelayMs * retryCount, _cts.Token));
                        if (_cts.IsCancellationRequested) {
                            // Done.
                            break;
                        }
                        if (retryCount++ > kMaxRetryCount) {
                            _logger.Error(ex,
                                "Error #{retryCount} in identity host - exit host...",
                                retryCount);
                            return;
                        }
                        _logger.Error(ex,
                            "Error #{retryCount} in identity host - restarting...",
                            retryCount);
                    }
                    finally {
                        _logger.Debug("Stopping host...");
                        Connected = false;
                        await host.StopAsync();
                        _logger.Information("Twin stopped.");
                        _started.TrySetResult(false); // Cancelled before started
                    }
                }
                _logger.Information("Exiting identity host.");
            }

            /// <summary>
            /// Create new connection string from existing EdgeHubConnectionString.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="deviceId"></param>
            /// <param name="secret"></param>
            /// <returns></returns>
            private static string GetEdgeHubConnectionString(IIoTEdgeConfig config,
                string deviceId, string secret) {

                var cs = config.EdgeHubConnectionString;
                if (string.IsNullOrEmpty(cs)) {
                    // Retrieve information from environment
                    var hostName = Environment.GetEnvironmentVariable("IOTEDGE_IOTHUBHOSTNAME");
                    if (string.IsNullOrEmpty(hostName)) {
                        throw new InvalidConfigurationException(
                            "Missing IOTEDGE_IOTHUBHOSTNAME variable in environment");
                    }
                    var edgeName = Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME");
                    cs = $"HostName={hostName};DeviceId={deviceId};SharedAccessKey={secret}";
                    if (!string.IsNullOrEmpty(edgeName)) {
                        cs += $";GatewayHostName={edgeName}";
                    }
                }
                else {
                    // Use existing connection string as a master plan
                    var lookup = cs
                        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().Split('='))
                        .ToDictionary(s => s[0].ToLowerInvariant(), v => v[1]);
                    if (!lookup.TryGetValue("hostname", out var hostName) ||
                        string.IsNullOrEmpty(hostName)) {
                        throw new InvalidConfigurationException(
                            "Missing HostName in connection string");
                    }

                    cs = $"HostName={hostName};DeviceId={deviceId};SharedAccessKey={secret}";
                    if (lookup.TryGetValue("GatewayHostName", out var edgeName) &&
                        !string.IsNullOrEmpty(edgeName)) {
                        cs += $";GatewayHostName={edgeName}";
                    }
                }
                return cs;
            }

            private const int kMaxRetryCount = 30;
            private const int kRetryDelayMs = 5000;

            private ILifetimeScope _container;
            private TaskCompletionSource<bool> _reset;
            private readonly TaskCompletionSource<bool> _started;
            private readonly IoTEdgeHostManager _outer;
            private readonly ILogger _logger;
            private readonly CancellationTokenSource _cts;
            private readonly Task _runner;
        }

        private readonly ILogger _logger;
        private readonly IIoTEdgeConfig _config;
        private readonly IContainerFactory _factory;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, IdentityHostProcess> _hosts =
            new Dictionary<string, IdentityHostProcess>();
    }
}

