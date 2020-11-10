﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Event bus host to auto inject handlers
    /// </summary>
    public class EventBusHost : IHostProcess {

        /// <summary>
        /// Auto registers handlers in client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public EventBusHost(IEventBus client, IEnumerable<IHandler> handlers, ILogger logger) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlers = handlers.ToDictionary(h => h, k => (string)null);
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_registration != null) {
                    throw new ResourceInvalidStateException("Event bus host already running.");
                }
                _registration = RegisterAsync();
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_registration != null) {
                    await _registration.ConfigureAwait(false);
                    await UnregisterAsync().ConfigureAwait(false);
                }
            }
            finally {
                _registration = null;
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            StopAsync().Wait();
            _lock.Dispose();
        }

        /// <summary>
        /// Unregister all handlers
        /// </summary>
        /// <returns></returns>
        private async Task UnregisterAsync() {
            foreach (var token in _handlers.Where(x => x.Value != null).ToList()) {
                try {
                    // Unregister using stored token
                    await _client.UnregisterAsync(token.Value).ConfigureAwait(false);
                    _handlers[token.Key] = null;
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Failed to stop Event bus host using token {token}.",
                        token);
                    throw;
                }
            }
            _logger.LogInformation("Event bus host stopped.");
        }

        /// <summary>
        /// Register all handlers
        /// </summary>
        /// <returns></returns>
        private async Task RegisterAsync() {
            var register = _client.GetType().GetMethod(nameof(IEventBus.RegisterAsync));
            foreach (var handler in _handlers.Keys.ToList()) {
                var type = handler.GetType();
                foreach (var itf in type.GetInterfaces()) {
                    try {
                        var eventType = itf.GetGenericArguments().FirstOrDefault();
                        if (eventType == null) {
                            continue;
                        }
                        var method = register.MakeGenericMethod(eventType);
                        _logger.LogDebug("Starting Event bus bridge for {type}...",
                            type.Name);
                        var token = await ((Task<string>)method.Invoke(_client,
                            new object[] { handler })).ConfigureAwait(false);
                        _handlers[handler] = token; // Store token to unregister
                        _logger.LogInformation("Event bus bridge for {type} started.",
                            type.Name);
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Failed to start Event bus host for {type}.",
                            type.Name);
                        throw;
                    }
                }
            }
            _logger.LogInformation("Event bus host running.");
        }

        private Task _registration;
        private readonly IEventBus _client;
        private readonly ILogger _logger;
        private readonly Dictionary<IHandler, string> _handlers;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    }
}