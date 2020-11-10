﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Http.SignalR {
    using Microsoft.Azure.IIoT.Http.SignalR.Services;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Hub client factory for signalr
    /// </summary>
    public sealed class SignalRHubClient : ICallbackClient, IDisposable, IAsyncDisposable {

        /// <summary>
        /// Create client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="provider"></param>
        /// <param name="settings"></param>
        public SignalRHubClient(IOptions<SignalRHubClientOptions> config, 
            ILogger logger, IJsonSerializerSettingsProvider settings = null,
            ITokenProvider provider = null) {
            _options = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings;
            _provider = provider;
            _clients = new Dictionary<string, SignalRClientRegistrar>();
            _lock = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc/>
        public async Task<ICallbackRegistrar> GetHubAsync(string endpointUrl,
            string resourceId) {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(SignalRHubClient));
            }
            if (string.IsNullOrEmpty(endpointUrl)) {
                throw new ArgumentNullException(nameof(endpointUrl));
            }
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                var lookup = endpointUrl;
                if (!string.IsNullOrEmpty(resourceId)) {
                    lookup += resourceId;
                }
                if (!_clients.TryGetValue(lookup, out var client) ||
                    client.ConnectionId == null) {
                    if (client != null) {
                        await client.DisposeAsync().ConfigureAwait(false);
                        _clients.Remove(lookup);
                    }
                    client = await SignalRClientRegistrar.CreateAsync(
                        _options.Value, endpointUrl, _logger, 
                        resourceId, _provider, _settings).ConfigureAwait(false);
                    _clients.Add(lookup, client);
                }
                return client;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            CloseAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            await CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <returns></returns>
        private async Task CloseAsync() {
            if (_disposed) {
                return;
            }
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                foreach (var client in _clients.Values) {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
                _clients.Clear();
            }
            finally {
                _lock.Release();
                _disposed = true;
            }
            _lock.Dispose();
        }

        /// <summary>
        /// SignalR client registry that manages consumed handles to it
        /// </summary>
        private sealed class SignalRClientRegistrar : ICallbackRegistrar {

            /// <inheritdoc/>
            public string ConnectionId {
                get {
                    if (_disposed) {
                        throw new ObjectDisposedException(nameof(SignalRClientRegistrar));
                    }
                    return _client.ConnectionId;
                }
            }

            private SignalRClientRegistrar(SignalRHubClientHost client) {
                _client = client;
            }

            /// <summary>
            /// Create instance by creating client host and starting it.
            /// </summary>
            /// <param name="options"></param>
            /// <param name="jsonSettings"></param>
            /// <param name="endpointUrl"></param>
            /// <param name="logger"></param>
            /// <param name="resourceId"></param>
            /// <param name="provider"></param>
            /// <returns></returns>
            internal static async Task<SignalRClientRegistrar> CreateAsync(
                SignalRHubClientOptions options, string endpointUrl, ILogger logger,
                string resourceId, ITokenProvider provider,
                IJsonSerializerSettingsProvider jsonSettings = null) {

                if (string.IsNullOrEmpty(endpointUrl)) {
                    throw new ArgumentNullException(nameof(endpointUrl));
                }
                var host = new SignalRHubClientHost(endpointUrl,
                    options.UseMessagePackProtocol, logger,
                    resourceId, provider, jsonSettings);

                await host.StartAsync().ConfigureAwait(false);
                return new SignalRClientRegistrar(host);
            }

            /// <inheritdoc/>
            public IDisposable Register(Func<object[], object, Task> handler,
                object thiz, string method, Type[] arguments) {
                if (_disposed) {
                    throw new ObjectDisposedException(nameof(SignalRClientRegistrar));
                }
                return _client.Register(handler, thiz, method, arguments);
            }

            /// <summary>
            /// Dispose
            /// </summary>
            /// <returns></returns>
            public async Task DisposeAsync() {
                if (_disposed) {
                    throw new ObjectDisposedException(nameof(SignalRClientRegistrar));
                }
                _disposed = true;
                await _client.StopAsync().ConfigureAwait(false);
            }

            private bool _disposed;
            private readonly SignalRHubClientHost _client;
        }

        private readonly IJsonSerializerSettingsProvider _settings;
        private readonly IOptions<SignalRHubClientOptions> _options;
        private readonly Dictionary<string, SignalRClientRegistrar> _clients;
        private readonly SemaphoreSlim _lock;
        private readonly ILogger _logger;
        private readonly ITokenProvider _provider;
        private bool _disposed;
    }
}