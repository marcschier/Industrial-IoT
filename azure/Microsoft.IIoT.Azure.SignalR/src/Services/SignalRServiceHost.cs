﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.SignalR.Services {
    using Microsoft.IIoT.Extensions.Rpc;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.Azure.SignalR.Management;
    using Microsoft.Azure.SignalR.Common;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Signalr service host for serverless
    /// </summary>
    public sealed class SignalRServiceHost<THub> : SignalRServiceEndpoint<THub>,
        ICallbackInvokerT<THub>, IGroupRegistrationT<THub>, IHostProcess,
        IHealthCheck, IDisposable where THub : Hub {

        /// <summary>
        /// Create signalR event bus
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public SignalRServiceHost(IOptions<SignalRServiceOptions> options, ILogger logger)
            : base(options) {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrEmpty(options.Value?.ConnectionString)) {
                throw new ArgumentException("Missing connection string", nameof(options));
            }
            if (!options.Value.IsServerLess) {
                throw new ArgumentException(
                    "SignalR is not configured to be in serverless mode according to " +
                    "the configuration. SignalR service should be configured serverless " +
                    "mode for the service host to work. Otherwise you should use " +
                    "Asp.net core as hosting environment.");
            }
            _renewHubTimer = new Timer(RenewHubTimer_ElapesedAsync);
            _renewHubInterval = TimeSpan.FromMinutes(3);
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken ct) {
            var hub = _hub;
            try {
                if (hub == null) {
                    hub = await _serviceManager.CreateHubContextAsync(Resource,
                        cancellationToken: ct).ConfigureAwait(false);
                }
                await hub.Clients.All.SendCoreAsync("ping", Array.Empty<object>(),
                    ct).ConfigureAwait(false);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex) {
                return new HealthCheckResult(context.Registration.FailureStatus,
                    exception: ex);
            }
            finally {
                if (hub != _hub) {
                    await hub.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            try {
                if (_hub != null) {
                    _logger.LogDebug("SignalR service host already running.");
                }
                else {
                    _logger.LogDebug("Starting SignalR service host...");
                    _hub = await _serviceManager.CreateHubContextAsync(Resource).ConfigureAwait(false);
                    _logger.LogInformation("SignalR service host started.");
                }
                // (re)start the timer no matter what
                Try.Op(() => _renewHubTimer.Change(
                    _renewHubInterval, Timeout.InfiniteTimeSpan));
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to start SignalR service host.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            try {
                _renewHubTimer.Change(
                    Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                if (_hub != null) {
                    _logger.LogDebug("Stopping SignalR service host...");
                    await _hub.DisposeAsync().ConfigureAwait(false);
                    _logger.LogInformation("SignalR service host stopped.");
                }
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to stop SignalR service host.");
            }
            finally {
                _hub = null;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastAsync(string method, object[] arguments,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(method)) {
                throw new ArgumentNullException(nameof(method));
            }
            try {
                await _hub.Clients.All.SendCoreAsync(method,
                    arguments ?? Array.Empty<object>(), ct).ConfigureAwait(false);
            }
            catch (AzureSignalRNotConnectedException e) {
                _logger.LogTrace(e,
                    "Failed to send broadcast message because hub is not connected");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to send broadcast message");
            }
        }

        /// <inheritdoc/>
        public async Task UnicastAsync(string target, string method,
            object[] arguments, CancellationToken ct) {
            if (string.IsNullOrEmpty(method)) {
                throw new ArgumentNullException(nameof(method));
            }
            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentNullException(nameof(target));
            }
            try {
                await _hub.Clients.User(target).SendCoreAsync(method,
                    arguments ?? Array.Empty<object>(), ct).ConfigureAwait(false);
            }
            catch (AzureSignalRNotConnectedException e) {
                _logger.LogTrace(e,
                    "Failed to send unicast message because hub is not connected");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to send unicast message");
            }
        }

        /// <inheritdoc/>
        public async Task MulticastAsync(string group, string method,
            object[] arguments, CancellationToken ct) {
            if (string.IsNullOrEmpty(method)) {
                throw new ArgumentNullException(nameof(method));
            }
            if (string.IsNullOrEmpty(group)) {
                throw new ArgumentNullException(nameof(group));
            }
            try {
                await _hub.Clients.Group(group).SendCoreAsync(method,
                    arguments ?? Array.Empty<object>(), ct).ConfigureAwait(false);
            }
            catch (AzureSignalRNotConnectedException e) {
                _logger.LogTrace(e,
                    "Failed to send multicast message because hub is not connected");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to send multicast message");
            }
        }

        /// <inheritdoc/>
        public Task SubscribeAsync(string group, string client,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(client)) {
                throw new ArgumentNullException(nameof(client));
            }
            if (string.IsNullOrEmpty(group)) {
                throw new ArgumentNullException(nameof(group));
            }
            return _hub.Groups.AddToGroupAsync(client, group, ct);
        }

        /// <inheritdoc/>
        public Task UnsubscribeAsync(string group, string client,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(client)) {
                throw new ArgumentNullException(nameof(client));
            }
            if (string.IsNullOrEmpty(group)) {
                throw new ArgumentNullException(nameof(group));
            }
            return _hub.Groups.RemoveFromGroupAsync(client, group, ct);
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
            _renewHubTimer.Dispose();
        }

        private async void RenewHubTimer_ElapesedAsync(object sender) {
            var hub = _hub;
            try {
                _hub = await _serviceManager.CreateHubContextAsync(Resource).ConfigureAwait(false);
            }
            finally {
                if (hub != _hub) {
                    await hub.DisposeAsync().ConfigureAwait(false);
                }
                Try.Op(() => _renewHubTimer.Change(
                    _renewHubInterval, Timeout.InfiniteTimeSpan));
            }
        }

        private IServiceHubContext _hub;
        private readonly Timer _renewHubTimer;
        private readonly TimeSpan _renewHubInterval;
        private readonly ILogger _logger;
    }
}