﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.SignalR.Services {
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// SignalR hub client
    /// </summary>
    public class SignalRHubClientHost : ICallbackRegistrar, IHostProcess {

        /// <inheritdoc/>
        public string ConnectionId => _connection.ConnectionId;

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <param name="useMessagePack"></param>
        /// <param name="jsonSettings"></param>
        /// <param name="msgPack"></param>
        /// <param name="logger"></param>
        /// <param name="resourceId"></param>
        /// <param name="provider"></param>
        public SignalRHubClientHost(string endpointUrl, bool? useMessagePack,
            ILogger logger, string resourceId, ITokenProvider provider = null,
            IJsonSerializerSettingsProvider jsonSettings = null,
            IMessagePackSerializerOptionsProvider msgPack = null) {
            if (string.IsNullOrEmpty(endpointUrl)) {
                throw new ArgumentNullException(nameof(endpointUrl));
            }
            _jsonSettings = jsonSettings;
            _msgPack = msgPack;
            _endpointUri = new Uri(endpointUrl);
            _useMessagePack = (useMessagePack ?? false) && _msgPack != null;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resourceId = provider?.Supports(resourceId) == true ? resourceId : null;
            _provider = provider;
        }

        /// <inheritdoc/>
        public IDisposable Register(Func<object[], object, Task> handler,
            object thiz, string method, Type[] arguments) {
            _lock.Wait();
            try {
                if (!_started) {
                    throw new InvalidOperationException("Must start before registering");
                }
                return _connection.On(method, arguments, handler, thiz);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_started) {
                    _logger.LogDebug("SignalR client host already running.");
                    return;
                }
                _logger.LogDebug("Starting SignalR client host...");
                _started = true;
                _connection = await OpenAsync().ConfigureAwait(false);
                _logger.LogInformation("SignalR client host started.");
            }
            catch (Exception ex) {
                _started = false;
                _logger.LogError(ex, "Error starting SignalR client host.");
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_started) {
                    return;
                }
                _started = false;
                _logger.LogDebug("Stopping SignalR client host...");
                await DisposeAsync(_connection).ConfigureAwait(false);
                _connection = null;
                _logger.LogInformation("SignalR client host stopped.");
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Error stopping SignalR client host.");
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            _lock.Wait();
            try {
                if (_connection != null) {
                    _logger.LogTrace("SignalR client was not stopped before disposing.");
                    Try.Op(() => DisposeAsync(_connection).Wait());
                    _connection = null;
                }
                _started = false;
            }
            finally {
                _lock.Release();
            }
            _lock.Dispose();
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <returns></returns>
        private async Task<HubConnection> OpenAsync() {
            var builder = new HubConnectionBuilder()
                .WithAutomaticReconnect();
            if (_useMessagePack && _msgPack != null) {
                builder = builder.AddMessagePackProtocol(options => {
                    options.SerializerOptions = _msgPack.Options;
                });
            }
            else {
                var jsonSettings = _jsonSettings?.Settings;
                if (jsonSettings != null) {
                    builder = builder.AddNewtonsoftJsonProtocol(options => {
                        options.PayloadSerializerSettings = jsonSettings;
                    });
                }
            }
            var connection = builder
                .WithUrl(_endpointUri, options => {
                    if (_provider != null) {
                        options.AccessTokenProvider = async () => {
                            var token = await _provider.GetTokenForAsync(_resourceId).ConfigureAwait(false);
                            if (token?.RawToken == null) {
                                _logger.LogError("Failed to aquire token for hub calling " +
                                    "({resource}) - calling without...",
                                    _resourceId);
                            }
                            return token?.RawToken;
                        };
                    }
                })
                .Build();
            connection.Closed += ex => OnClosedAsync(connection, ex);
            await connection.StartAsync().ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <returns></returns>
        private static async Task DisposeAsync(HubConnection connection) {
            if (connection == null) {
                return;
            }
            await Try.Async(() => connection?.StopAsync()).ConfigureAwait(false);
            await Try.Async(() => connection?.DisposeAsync().AsTask()).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle close event
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task OnClosedAsync(HubConnection connection, Exception ex) {
            _logger.LogError(ex, "SignalR client host Disconnected!");
            await DisposeAsync(connection).ConfigureAwait(false);
            if (_started) {
                // Reconnect
                _connection = await OpenAsync().ConfigureAwait(false);
                _logger.LogInformation("SignalR client host reconnecting...");
            }
        }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IJsonSerializerSettingsProvider _jsonSettings;
        private readonly IMessagePackSerializerOptionsProvider _msgPack;
        private readonly Uri _endpointUri;
        private readonly bool _useMessagePack;
        private readonly ILogger _logger;
        private readonly ITokenProvider _provider;
        private readonly string _resourceId;
        private HubConnection _connection;
        private bool _started;
    }
}