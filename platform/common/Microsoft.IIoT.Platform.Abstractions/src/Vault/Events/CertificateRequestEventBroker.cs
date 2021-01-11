// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Vault.Events {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Tasks;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    /// <summary>
    /// Certificate Request event broker - publishes locally, and also
    /// all event versions to event bus
    /// </summary>
    public sealed class CertificateRequestEventBroker :
        ICertificateRequestEventBroker, ICertificateRequestEvents {

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="processor"></param>
        public CertificateRequestEventBroker(IEventBusPublisher bus, ITaskProcessor processor = null) {
            _processor = processor;
            _listeners = new ConcurrentDictionary<string, ICertificateRequestListener>();

            _listeners.TryAdd("v2", new v2.CertificateRequestEventPublisher(bus));
            // ...
        }

        /// <inheritdoc/>
        public Action Register(ICertificateRequestListener listener) {
            var token = Guid.NewGuid().ToString();
            _listeners.TryAdd(token, listener);
            return () => _listeners.TryRemove(token, out var _);
        }

        /// <inheritdoc/>
        public Task NotifyAllAsync(Func<ICertificateRequestListener, Task> evt) {
            Task task() => Task
                .WhenAll(_listeners.Values.Select(l => evt(l)).ToArray());
            if (_processor == null || !_processor.TrySchedule(task)) {
                return task().ContinueWith(t => Task.CompletedTask);
            }
            return Task.CompletedTask;
        }

        private readonly ITaskProcessor _processor;
        private readonly ConcurrentDictionary<string, ICertificateRequestListener> _listeners;
    }
}
