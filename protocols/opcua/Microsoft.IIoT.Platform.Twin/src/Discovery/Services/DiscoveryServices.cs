// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Services {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Platform.Discovery.Clients;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Platform.OpcUa.Transport.Probe;
    using Microsoft.IIoT.Platform.OpcUa.Models;
    using Microsoft.IIoT.Platform.OpcUa;
    using Microsoft.IIoT.Extensions.Net.Scanner;
    using Microsoft.IIoT.Extensions.Net.Models;
    using Microsoft.IIoT.Extensions.Net;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Serializers;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides discovery services
    /// </summary>
    public sealed class DiscoveryServices : IDiscoveryServices, INetworkScanner,
        IDisposable {

        /// <inheritdoc/>
        public DiscoveryMode Mode => _request.Mode;

        /// <inheritdoc/>
        public DiscoveryConfigModel Configuration => _request.Configuration;

        /// <summary>
        /// Create services
        /// </summary>
        /// <param name="client"></param>
        /// <param name="publish"></param>
        /// <param name="logger"></param>
        /// <param name="serializer"></param>
        /// <param name="progress"></param>
        public DiscoveryServices(IEndpointDiscovery client, IJsonSerializer serializer,
            IDiscoveryResultHandler publish, ILogger logger,
            IDiscoveryProgressHandler progress = null) {

            _publish = publish ?? throw new ArgumentNullException(nameof(publish));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _progress = progress ?? new DiscoveryProgressLogger(logger);

            _runner = Task.Run(() => ProcessDiscoveryRequestsAsync(_cts.Token));
            _timer = new Timer(_ => OnScanScheduling(), null,
                TimeSpan.FromSeconds(20), Timeout.InfiniteTimeSpan);
        }

        /// <inheritdoc/>
        public Task ConfigureAsync(DiscoveryMode mode, DiscoveryConfigModel config) {
            _request = new DiscoveryRequest(mode, config);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(ServerRegistrationRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.DiscoveryUrl == null) {
                throw new ArgumentException("Missing discovery uri", nameof(request));
            }
            if (string.IsNullOrEmpty(request.Id)) {
                request.Id = Guid.NewGuid().ToString();
            }
            await DiscoverAsync(new DiscoveryRequestModel {
                Configuration = new DiscoveryConfigModel {
                    DiscoveryUrls = new List<string> { request.DiscoveryUrl },
                },
                Id = request.Id,
                Context = context
            }, context, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DiscoverAsync(DiscoveryRequestModel request,
            OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var task = new DiscoveryRequest(request);
            var scheduled = _queue.TryAdd(task);
            if (!scheduled) {
                task.Dispose();
                _logger.LogError("Discovey request not scheduled, internal server error!");
                var ex = new ResourceExhaustionException("Failed to schedule task");
                _progress.OnDiscoveryError(request, ex);
                throw ex;
            }
            await _lock.WaitAsync(ct).ConfigureAwait(false);
            try {
                if (_pending.Count != 0) {
                    _progress.OnDiscoveryPending(task.Request, _pending.Count);
                }
                _pending.Add(task);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task CancelAsync(DiscoveryCancelModel request,
            OperationContextModel context, CancellationToken ct) {
            context = context.Validate();
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _lock.WaitAsync(ct).ConfigureAwait(false);
            try {
                foreach (var task in _pending.Where(r => r.Request.Id == request.Id)) {
                    // Cancel the task
                    task.Cancel();
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public Task ScanAsync() {
            // Fire timer now so that new request is scheduled
            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(StopDiscoveryRequestProcessingAsync).Wait();

            // Dispose
            _cts.Dispose();
            _timer.Dispose();
            _lock.Dispose();
        }

        /// <summary>
        /// Scan timer expired
        /// </summary>
        private void OnScanScheduling() {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _lock.Wait();
            try {
                foreach (var task in _pending.Where(r => r.IsScan)) {
                    // Cancel any current scan tasks if any
                    task.Cancel();
                }

                // Add new discovery request
                if (Mode != DiscoveryMode.Off) {
                    // Push request
                    var task = _request.Clone();
                    if (_queue.TryAdd(task)) {
                        _pending.Add(task);
                    }
                    else {
                        task.Dispose();
                    }
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Stop discovery request processing
        /// </summary>
        /// <returns></returns>
        private async Task StopDiscoveryRequestProcessingAsync() {
            _queue.CompleteAdding();
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                // Cancel all requests first
                foreach (var request in _pending) {
                    request.Cancel();
                }
            }
            finally {
                _lock.Release();
            }

            // Try cancel discovery and wait for completion of runner
            Try.Op(() => _cts?.Cancel());
            try {
                await _runner.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) {
                _logger.LogError(ex, "Unexpected exception stopping processor thread.");
            }
        }

        /// <summary>
        /// Process discovery requests
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ProcessDiscoveryRequestsAsync(CancellationToken ct) {
            _logger.LogInformation("Starting discovery processor...");
            // Process all discovery requests
            while (!ct.IsCancellationRequested) {
                try {
                    var request = _queue.Take(ct);
                    try {
                        // Update pending queue size
                        await ReportPendingRequestsAsync().ConfigureAwait(false);
                        await ProcessDiscoveryRequestAsync(request).ConfigureAwait(false);
                    }
                    finally {
                        // If the request is scan request, schedule next one
                        if (!ct.IsCancellationRequested && (request?.IsScan ?? false)) {
                            // Re-schedule another scan when idle time expired
                            _timer.Change(
                                request.Request.Configuration.IdleTimeBetweenScans ??
                                    TimeSpan.FromHours(1),
                                Timeout.InfiniteTimeSpan);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) {
                    _logger.LogError(ex, "Discovery processor error occurred - continue...");
                }
            }
            // Send cancellation for all pending items
            await CancelPendingRequestsAsync().ConfigureAwait(false);
            _logger.LogInformation("Stopped discovery processor.");
        }

        /// <summary>
        /// Process the provided discovery request
        /// </summary>
        /// <param name="request"></param>
        private async Task ProcessDiscoveryRequestAsync(DiscoveryRequest request) {
            _logger.LogDebug("Processing discovery request...");
            _progress.OnDiscoveryStarted(request.Request);
            object diagnostics = null;

            //
            // Discover servers
            //
            List<ApplicationRegistrationModel> discovered;
            try {
                discovered = await DiscoverServersAsync(request).ConfigureAwait(false);
                request.Token.ThrowIfCancellationRequested();
                //
                // Upload results
                //
                await SendDiscoveryResultsAsync(request, discovered, DateTime.UtcNow,
                    diagnostics, request.Token).ConfigureAwait(false);

                _progress.OnDiscoveryFinished(request.Request);
            }
            catch (OperationCanceledException) {
                _progress.OnDiscoveryCancelled(request.Request);
            }
            catch (Exception ex) {
                _progress.OnDiscoveryError(request.Request, ex);
            }
            finally {
                if (request != null) {
                    await _lock.WaitAsync().ConfigureAwait(false);
                    try {
                        _pending.Remove(request);
                        Try.Op(() => request.Dispose());
                    }
                    finally {
                        _lock.Release();
                    }
                }
            }
        }

        /// <summary>
        /// Upload results
        /// </summary>
        /// <param name="discovered"></param>
        /// <param name="request"></param>
        /// <param name="timestamp"></param>
        /// <param name="diagnostics"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        internal async Task SendDiscoveryResultsAsync(DiscoveryRequest request,
            List<ApplicationRegistrationModel> discovered, DateTime timestamp,
            object diagnostics, CancellationToken ct) {
            _logger.LogInformation("Uploading {count} results...", discovered.Count);
            var messages = discovered
                .SelectMany(server => server.Endpoints
                    .Select(endpoint => new DiscoveryResultModel {
                        Application = server.Application,
                        Endpoint = endpoint,
                        TimeStamp = timestamp
                    }))
                .Append(new DiscoveryResultModel {
                    Endpoint = null, // last
                    Result = new DiscoveryContextModel {
                        DiscoveryConfig = request.Configuration,
                        Id = request.Request.Id,
                        Context = request.Request.Context.Clone(),
                        RegisterOnly = request.Mode == DiscoveryMode.Off,
                        Diagnostics = diagnostics == null ? null :
                            _serializer.FromObject(diagnostics)
                    },
                    TimeStamp = timestamp
                })
                .Select((discovery, i) => {
                    discovery.Index = i;
                    return discovery;
                });
            await _publish.ReportResultsAsync(messages, ct).ConfigureAwait(false);
            _logger.LogInformation("{count} results uploaded.", discovered.Count);
        }

        /// <summary>
        /// Run a network discovery
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<List<ApplicationRegistrationModel>> DiscoverServersAsync(
            DiscoveryRequest request) {
            var discoveryUrls = await GetDiscoveryUrlsAsync(request.DiscoveryUrls).ConfigureAwait(false);

            _logger.LogInformation("Start {mode} discovery run...", request.Mode);
            var watch = Stopwatch.StartNew();

            if (request.Mode == DiscoveryMode.Url) {
                var discoveredUrl = await DiscoverServersAsync(request, discoveryUrls,
                    request.Configuration.Locales).ConfigureAwait(false);

                _logger.LogInformation("Discovery took {elapsed} and found {count} servers.",
                watch.Elapsed, discoveredUrl.Count);
                return discoveredUrl;
            }

            //
            // Set up scanner pipeline and start discovery
            //
            var local = request.Mode == DiscoveryMode.Local;
#if !NO_WATCHDOG
            _counter = 0;
#endif
            var addresses = new List<IPAddress>();
            _progress.OnNetScanStarted(request.Request, 0, 0, request.TotalAddresses);
            using (var netscanner = new NetworkScanner(_logger, (scanner, reply) => {
                _progress.OnNetScanResult(request.Request, scanner.ActiveProbes,
                    scanner.ScanCount, request.TotalAddresses, addresses.Count, reply.Address);
                addresses.Add(reply.Address);
            }, local, local ? null : request.AddressRanges, request.NetworkClass,
                request.Configuration.MaxNetworkProbes, request.Configuration.NetworkProbeTimeout,
                request.Token)) {

                // Log progress
                using (var progress = new Timer(_ => ProgressTimer(
                    () => _progress.OnNetScanProgress(request.Request, netscanner.ActiveProbes,
                        netscanner.ScanCount, request.TotalAddresses, addresses.Count)),
                    null, kProgressInterval, kProgressInterval)) {
                    await netscanner.Completion.ConfigureAwait(false);
                }
                _progress.OnNetScanFinished(request.Request, netscanner.ActiveProbes,
                    netscanner.ScanCount, request.TotalAddresses, addresses.Count);
            }
            request.Token.ThrowIfCancellationRequested();

            await AddLoopbackAddressesAsync(addresses).ConfigureAwait(false);
            if (addresses.Count == 0) {
                return new List<ApplicationRegistrationModel>();
            }

            var ports = new List<IPEndPoint>();
            var totalPorts = request.TotalPorts * addresses.Count;
            var probe = new ServerProbe(_logger);
#if !NO_WATCHDOG
            _counter = 0;
#endif
            _progress.OnPortScanStart(request.Request, 0, 0, totalPorts);
            using (var portscan = new PortScanner(_logger,
                addresses.SelectMany(address => {
                    var ranges = request.PortRanges ?? PortRange.OpcUa;
                    return ranges.SelectMany(x => x.GetEndpoints(address));
                }), (scanner, ep) => {
                    _progress.OnPortScanResult(request.Request, scanner.ActiveProbes,
                        scanner.ScanCount, totalPorts, ports.Count, ep);
                    ports.Add(ep);
                }, probe, request.Configuration.MaxPortProbes,
                request.Configuration.MinPortProbesPercent,
                request.Configuration.PortProbeTimeout, request.Token)) {

                using (var progress = new Timer(_ => ProgressTimer(
                    () => _progress.OnPortScanProgress(request.Request, portscan.ActiveProbes,
                        portscan.ScanCount, totalPorts, ports.Count)),
                    null, kProgressInterval, kProgressInterval)) {
                    await portscan.Completion.ConfigureAwait(false);
                }
                _progress.OnPortScanFinished(request.Request, portscan.ActiveProbes,
                    portscan.ScanCount, totalPorts, ports.Count);
            }
            request.Token.ThrowIfCancellationRequested();
            if (ports.Count == 0) {
                return new List<ApplicationRegistrationModel>();
            }

            //
            // Collect discovery urls
            //
            foreach (var ep in ports) {
                request.Token.ThrowIfCancellationRequested();
                var resolved = await ep.TryResolveAsync().ConfigureAwait(false);
                var url = new Uri($"opc.tcp://" + resolved);
                discoveryUrls.Add(ep, url);
            }
            request.Token.ThrowIfCancellationRequested();

            //
            // Create application model list from discovered endpoints...
            //
            var discovered = await DiscoverServersAsync(request, discoveryUrls,
                request.Configuration.Locales).ConfigureAwait(false);

            _logger.LogInformation("Discovery took {elapsed} and found {count} servers.",
                watch.Elapsed, discovered.Count);
            return discovered;
        }

        /// <summary>
        /// Discover servers using opcua discovery and filter by optional locale
        /// </summary>
        /// <param name="request"></param>
        /// <param name="discoveryUrls"></param>
        /// <param name="locales"></param>
        /// <returns></returns>
        private async Task<List<ApplicationRegistrationModel>> DiscoverServersAsync(
            DiscoveryRequest request, IReadOnlyDictionary<IPEndPoint, Uri> discoveryUrls,
            IReadOnlyList<string> locales) {
            var discovered = new List<ApplicationRegistrationModel>();
            var count = 0;
            _progress.OnServerDiscoveryStarted(request.Request, 1, count, discoveryUrls.Count);
            foreach (var item in discoveryUrls) {
                request.Token.ThrowIfCancellationRequested();
                var url = item.Value;

                _progress.OnFindEndpointsStarted(request.Request, 1, count, discoveryUrls.Count,
                    discovered.Count, url, item.Key.Address);

                // Find endpoints at the real accessible ip address
                var eps = await _client.FindEndpointsAsync(new UriBuilder(url) {
                    Host = item.Key.Address.ToString()
                }.Uri, locales, request.Token).ConfigureAwait(false);

                count++;
                var endpoints = 0;
                foreach (var ep in eps) {
                    discovered.AddOrUpdate(ep.ToServiceModel(item.Key.ToString(), _serializer));
                    endpoints++;
                }
                _progress.OnFindEndpointsFinished(request.Request, 1, count, discoveryUrls.Count,
                    discovered.Count, url, item.Key.Address, endpoints);
            }

            _progress.OnServerDiscoveryFinished(request.Request, 1, discoveryUrls.Count,
                discoveryUrls.Count, discovered.Count);
            request.Token.ThrowIfCancellationRequested();
            return discovered;
        }

        /// <summary>
        /// Get all reachable addresses from urls
        /// </summary>
        /// <param name="discoveryUrls"></param>
        /// <returns></returns>
        private async Task<Dictionary<IPEndPoint, Uri>> GetDiscoveryUrlsAsync(
            IEnumerable<Uri> discoveryUrls) {
            if (discoveryUrls?.Any() ?? false) {
                var results = await Task.WhenAll(discoveryUrls
                    .Select(GetHostEntryAsync)
                    .ToArray()).ConfigureAwait(false);
                return results
                    .SelectMany(v => v)
                    .Where(a => a.Item2 != null)
                    .ToDictionary(k => k.Item1, v => v.Item2);
            }
            return new Dictionary<IPEndPoint, Uri>();
        }

        /// <summary>
        /// Get a reachable host address from url
        /// </summary>
        /// <param name="discoveryUrl"></param>
        /// <returns></returns>
        private Task<List<Tuple<IPEndPoint, Uri>>> GetHostEntryAsync(
            Uri discoveryUrl) {
            return Try.Async(async () => {
                var host = discoveryUrl.DnsSafeHost;
                var list = new List<Tuple<IPEndPoint, Uri>>();

                // check first if host is an IP Address since the Dns.GetHostEntryAsync
                // throws a socket exception when called with an IP address
                try {
                    var hostIp = IPAddress.Parse(host);
                    var ep = new IPEndPoint(hostIp,
                            discoveryUrl.IsDefaultPort ? 4840 : discoveryUrl.Port);
                    list.Add(Tuple.Create(ep, discoveryUrl));
                    return list;
                }
                catch {
                    // Parsing failed, therefore not an IP address, continue with dns
                    // resolution
                }

                while (!string.IsNullOrEmpty(host)) {
                    try {
                        var entry = await Dns.GetHostEntryAsync(host).ConfigureAwait(false);
                        // only pick-up the IPV4 addresses
                        var foundIpv4 = false;
                        foreach (var address in entry.AddressList
                            .Where(a => a.AddressFamily == AddressFamily.InterNetwork)) {
                            var ep = new IPEndPoint(address,
                                discoveryUrl.IsDefaultPort ? 4840 : discoveryUrl.Port);
                            list.Add(Tuple.Create(ep, discoveryUrl));
                            foundIpv4 = true;
                        }
                        if (!foundIpv4) {
                            // if no IPV4 responsive, try IPV6 as fallback
                            foreach (var address in entry.AddressList
                                .Where(a => a.AddressFamily != AddressFamily.InterNetwork)) {
                                var ep = new IPEndPoint(address,
                                    discoveryUrl.IsDefaultPort ? 4840 : discoveryUrl.Port);
                                list.Add(Tuple.Create(ep, discoveryUrl));
                            }
                        }

                        // Check local host
                        if (host.EqualsIgnoreCase("localhost") && Host.IsContainer) {
                            // Also resolve docker internal since we are in a container
                            host = kDockerHostName;
                            continue;
                        }
                        break;
                    }
                    catch (Exception e) {
                        _logger.LogWarning(e, "Failed to resolve the host for {discoveryUrl}", discoveryUrl);
                        return list;
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// Add localhost ip to list if not already in it.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private async Task AddLoopbackAddressesAsync(List<IPAddress> addresses) {
            // Check local host
            try {
                if (Host.IsContainer) {
                    // Resolve docker host since we are running in a container
                    var entry = await Dns.GetHostEntryAsync(kDockerHostName).ConfigureAwait(false);
                    foreach (var address in entry.AddressList
                                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                                .Where(a => !addresses.Any(b => a.Equals(b)))) {
                        _logger.LogInformation("Including host address {address}", address);
                        addresses.Add(address);
                    }
                }
                else {
                    // Add loopback address
                    addresses.Add(IPAddress.Loopback);
                }
            }
            catch (Exception e) {
                _logger.LogWarning(e, "Failed to add local host address.");
            }
        }

        /// <summary>
        /// Cancel all remaining pending requests
        /// </summary>
        /// <returns></returns>
        private async Task CancelPendingRequestsAsync() {
            _logger.LogInformation("Cancelling all pending requests...");
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                foreach (var request in _pending) {
                    _progress.OnDiscoveryCancelled(request.Request);
                    Try.Op(() => request.Dispose());
                }
                _pending.Clear();
            }
            finally {
                _lock.Release();
            }
            _logger.LogInformation("Pending requests cancelled...");
        }

        /// <summary>
        /// Send pending queue size
        /// </summary>
        /// <returns></returns>
        private async Task ReportPendingRequestsAsync() {
            // Notify all listeners about the request's place in queue
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                for (var pos = 0; pos < _pending.Count; pos++) {
                    var item = _pending[pos];
                    if (!item.Token.IsCancellationRequested) {
                        _progress.OnDiscoveryPending(item.Request, pos);
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to send pending event");
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Called in intervals to check and log progress.
        /// </summary>
        /// <param name="log"></param>
        private void ProgressTimer(Action log) {
            if ((_counter % 3) == 0) {
                _logger.LogInformation("GC Mem: {gcmem} kb, Working set / Private Mem: " +
                    "{privmem} kb / {privmemsize} kb, Handles: {handles}",
                    GC.GetTotalMemory(false) / 1024,
                    Process.GetCurrentProcess().WorkingSet64 / 1024,
                    Process.GetCurrentProcess().PrivateMemorySize64 / 1024,
                    Process.GetCurrentProcess().HandleCount);
            }
            ++_counter;
#if !NO_WATCHDOG
            if ((_counter % 200) == 0) {
                if (_counter >= 2000) {
                    throw new ThreadStateException("Stuck");
                }
            }
#endif
            log();
        }
#if !NO_WATCHDOG
        private int _counter;
#endif

        /// <summary>
        /// Discovery request wrapper
        /// </summary>
        internal sealed class DiscoveryRequest : IDisposable {

            /// <summary>
            /// Cancellation token to cancel request
            /// </summary>
            public CancellationToken Token => _cts.Token;

            /// <summary>
            /// Request is a scan request
            /// </summary>
            public bool IsScan { get; }

            /// <summary>
            /// Original discovery request model
            /// </summary>
            public DiscoveryRequestModel Request { get; }

            /// <summary>
            /// Network class
            /// </summary>
            public NetworkClass NetworkClass { get; }

            /// <summary>
            /// Address ranges to use or null to use from network info
            /// </summary>
            public IEnumerable<AddressRange> AddressRanges { get; }

            /// <summary>
            /// Total addresses to be scanned
            /// </summary>
            public int TotalAddresses { get; }

            /// <summary>
            /// Port ranges to use if not from discovery mode
            /// </summary>
            public IEnumerable<PortRange> PortRanges { get; }

            /// <summary>
            /// Total ports to be scanned
            /// </summary>
            public int TotalPorts { get; }

            /// <summary>
            /// Discovery mode
            /// </summary>
            public DiscoveryMode Mode =>
                Request.Discovery ?? DiscoveryMode.Off;

            /// <summary>
            /// Discovery configuration
            /// </summary>
            public DiscoveryConfigModel Configuration =>
                Request.Configuration ?? new DiscoveryConfigModel();

            /// <summary>
            /// Discovery urls
            /// </summary>
            public IEnumerable<Uri> DiscoveryUrls =>
                Configuration.DiscoveryUrls?.Select(s => new Uri(s)) ??
                    Enumerable.Empty<Uri>();

            /// <summary>
            /// Create request wrapper
            /// </summary>
            public DiscoveryRequest() :
                this(null, null) {
            }

            /// <summary>
            /// Create request wrapper
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="configuration"></param>
            public DiscoveryRequest(DiscoveryMode? mode,
                DiscoveryConfigModel configuration) :
                this(new DiscoveryRequestModel {
                    Id = "",
                    Configuration = configuration,
                    Context = null,
                    Discovery = mode
                }, NetworkClass.Wired, true) {
            }

            /// <summary>
            /// Create request wrapper
            /// </summary>
            /// <param name="request"></param>
            /// <param name="networkClass"></param>
            /// <param name="isScan"></param>
            public DiscoveryRequest(DiscoveryRequestModel request,
                NetworkClass networkClass = NetworkClass.Wired, bool isScan = false) {
                Request = request?.Clone() ?? throw new ArgumentNullException(nameof(request));
                _cts = new CancellationTokenSource();
                NetworkClass = networkClass;
                IsScan = isScan;

                if (Request.Configuration == null) {
                    Request.Configuration = new DiscoveryConfigModel();
                }

                if (Request.Discovery == null ||
                    Request.Discovery == DiscoveryMode.Off) {
                    // Report empty configuration if off, but keep the
                    // discovery urls details from the original request
                    Request.Configuration = new DiscoveryConfigModel() {
                        DiscoveryUrls = Request.Configuration.DiscoveryUrls?.ToList(),
                        Locales = Request.Configuration.Locales?.ToList()
                    };
                    Request.Discovery = DiscoveryMode.Off;
                    return;
                }

                // Parse whatever provided

                if (!string.IsNullOrEmpty(Request.Configuration.PortRangesToScan)) {
                    if (PortRange.TryParse(Request.Configuration.PortRangesToScan,
                        out var ports)) {
                        PortRanges = ports;
                        if (Request.Discovery == null) {
                            Request.Discovery = DiscoveryMode.Fast;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Request.Configuration.AddressRangesToScan)) {
                    if (AddressRange.TryParse(Request.Configuration.AddressRangesToScan,
                        out var addresses)) {
                        AddressRanges = addresses;
                        if (Request.Discovery == null) {
                            Request.Discovery = DiscoveryMode.Fast;
                        }
                    }
                }

                // Set default ranges

                if (AddressRanges == null) {
                    IEnumerable<NetInterface> interfaces;
                    switch (Request.Discovery) {
                        case DiscoveryMode.Local:
                            interfaces = NetworkInformationEx.GetAllNetInterfaces(NetworkClass);
                            AddressRanges = AddLocalHost(interfaces
                                .Select(t => new AddressRange(t, true)))
                                .Distinct();
                            break;
                        case DiscoveryMode.Fast:
                            interfaces = NetworkInformationEx.GetAllNetInterfaces(NetworkClass.Wired);
                            AddressRanges = AddLocalHost(interfaces
                                .Select(t => new AddressRange(t, false, 24))
                                .Concat(interfaces
                                    .Where(t => t.Gateway != null &&
                                                !t.Gateway.Equals(IPAddress.Any) &&
                                                !t.Gateway.Equals(IPAddress.None))
                                    .Select(i => new AddressRange(i.Gateway, 32)))
                                .Distinct());
                            break;
                        case DiscoveryMode.Network:
                        case DiscoveryMode.Scan:
                            interfaces = NetworkInformationEx.GetAllNetInterfaces(NetworkClass);
                            AddressRanges = AddLocalHost(interfaces
                                .Select(t => new AddressRange(t, false))
                                .Concat(interfaces
                                    .Where(t => t.Gateway != null &&
                                                !t.Gateway.Equals(IPAddress.Any) &&
                                                !t.Gateway.Equals(IPAddress.None))
                                    .Select(i => new AddressRange(i.Gateway, 32)))
                                .Distinct());
                            break;
                        case DiscoveryMode.Off:
                        case DiscoveryMode.Url:
                        default:
                            AddressRanges = Enumerable.Empty<AddressRange>();
                            break;
                    }
                }

                if (PortRanges == null) {
                    switch (Request.Discovery) {
                        case DiscoveryMode.Local:
                            PortRanges = PortRange.All;
                            break;
                        case DiscoveryMode.Fast:
                            PortRanges = PortRange.WellKnown;
                            break;
                        case DiscoveryMode.Scan:
                            PortRanges = PortRange.Unassigned;
                            break;
                        case DiscoveryMode.Network:
                            PortRanges = PortRange.OpcUa;
                            break;
                        case DiscoveryMode.Off:
                        case DiscoveryMode.Url:
                        default:
                            PortRanges = Enumerable.Empty<PortRange>();
                            break;
                    }
                }

                // Update reported configuration with used settings

                if (AddressRanges != null && AddressRanges.Any()) {
                    Request.Configuration.AddressRangesToScan = AddressRange.Format(AddressRanges);
                    TotalAddresses = AddressRanges?.Sum(r => r.Count) ?? 0;
                }

                if (PortRanges != null && PortRanges.Any()) {
                    Request.Configuration.PortRangesToScan = PortRange.Format(PortRanges);
                    TotalPorts = PortRanges?.Sum(r => r.Count) ?? 0;
                }

                Request.Configuration.IdleTimeBetweenScans ??= kDefaultIdleTime;
                Request.Configuration.PortProbeTimeout ??= kDefaultPortProbeTimeout;
                Request.Configuration.NetworkProbeTimeout ??= kDefaultNetworkProbeTimeout;
            }

            /// <summary>
            /// Create request wrapper
            /// </summary>
            /// <param name="request"></param>
            public DiscoveryRequest(DiscoveryRequest request) :
                this(request.Request, request.NetworkClass, request.IsScan) {
            }

            /// <summary>
            /// Cancel request
            /// </summary>
            public void Cancel() {
                Try.Op(() => _cts.Cancel());
            }

            /// <inheritdoc/>
            public void Dispose() {
                _cts.Dispose();
            }

            /// <summary>
            /// Clone options
            /// </summary>
            /// <returns></returns>
            internal DiscoveryRequest Clone() {
                return new DiscoveryRequest(this);
            }

            /// <summary>
            /// Add hosta address as fake address range
            /// </summary>
            /// <param name="ranges"></param>
            /// <returns></returns>
            public static IEnumerable<AddressRange> AddLocalHost(IEnumerable<AddressRange> ranges) {
                if (Host.IsContainer) {
                    try {
                        var addresses = Dns.GetHostAddresses("host.docker.internal");
                        var listedRanges = ranges.ToList();
                        return listedRanges.Concat(addresses
                            // Select ip4 addresses only
                            .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                            .Select(a => new IPv4Address(a))
                            // Check we do not already have them in the existing ranges
                            .Where(a => !listedRanges
                                .Any(r => a >= r.Low && a <= r.High))
                            // Select either the local or a small subnet around it
                            .Select(a => new AddressRange(a, 32, "localhost")));
                    }
                    catch {
                    }
                }
                return ranges;
            }

            /// <summary> Default idle time is 6 hours </summary>
            private static readonly TimeSpan kDefaultIdleTime = TimeSpan.FromHours(6);
            /// <summary> Default port probe timeout is 5 seconds </summary>
            private static readonly TimeSpan kDefaultPortProbeTimeout = TimeSpan.FromSeconds(5);
            /// <summary> Default icmp timeout is 3 seconds </summary>
            private static readonly TimeSpan kDefaultNetworkProbeTimeout = TimeSpan.FromSeconds(3);

            private readonly CancellationTokenSource _cts;
        }

        /// <summary> Progress reporting every 3 seconds </summary>
        private static readonly TimeSpan kProgressInterval = TimeSpan.FromSeconds(3);
        private const string kDockerHostName = "host.docker.internal";

        private readonly ILogger _logger;
        private readonly IDiscoveryResultHandler _publish;
        private readonly IJsonSerializer _serializer;
        private readonly IDiscoveryProgressHandler _progress;
        private readonly IEndpointDiscovery _client;
        private readonly Task _runner;
        private readonly Timer _timer;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly List<DiscoveryRequest> _pending =
            new List<DiscoveryRequest>();
        private readonly BlockingCollection<DiscoveryRequest> _queue =
            new BlockingCollection<DiscoveryRequest>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private DiscoveryRequest _request = new DiscoveryRequest();
    }
}
