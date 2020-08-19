﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Default {
    using Microsoft.Azure.IIoT.Messaging;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    /// <summary>
    /// Simple in memory event bus
    /// </summary>
    public class SimpleEventBus : IEventBus {

        /// <inheritdoc/>
        public Task PublishAsync<T>(T message) {
            var name = typeof(T).GetMoniker();
            _handlers.TryGetValue(name, out var handler);
            return ((IEventHandler<T>)handler).HandleAsync(message);
        }

        /// <inheritdoc/>
        public Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            var token = typeof(T).GetMoniker();
            _handlers.AddOrUpdate(token, handler);
            return Task.FromResult(token);
        }

        /// <inheritdoc/>
        public Task UnregisterAsync(string token) {
            _handlers.Remove(token);
            return Task.CompletedTask;
        }

        private readonly Dictionary<string, object> _handlers =
            new Dictionary<string, object>();
    }
}