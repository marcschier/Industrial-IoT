﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher.Default {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Tasks;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    /// <summary>
    /// WriterGroup event broker - publishes locally, and also
    /// all event versions to event bus
    /// </summary>
    public sealed class WriterGroupEventBroker :
        IPublisherEventBroker<IWriterGroupRegistryListener>,
        IPublisherEvents<IWriterGroupRegistryListener> {

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="processor"></param>
        public WriterGroupEventBroker(IEventBusPublisher bus, ITaskProcessor processor = null) {
            _processor = processor;
            _listeners = new ConcurrentDictionary<string, IWriterGroupRegistryListener>();

            _listeners.TryAdd("v2", new Events.v2.WriterGroupEventBusPublisher(bus));
            // ...
        }

        /// <inheritdoc/>
        public Action Register(IWriterGroupRegistryListener listener) {
            var token = Guid.NewGuid().ToString();
            _listeners.TryAdd(token, listener);
            return () => _listeners.TryRemove(token, out var _);
        }

        /// <inheritdoc/>
        public Task NotifyAllAsync(Func<IWriterGroupRegistryListener, Task> evt) {
            Task task() => Task
                .WhenAll(_listeners.Values.Select(l => evt(l)).ToArray());
            if (_processor == null || !_processor.TrySchedule(task)) {
                return task().ContinueWith(t => Task.CompletedTask);
            }
            return Task.CompletedTask;
        }

        private readonly ITaskProcessor _processor;
        private readonly ConcurrentDictionary<string, IWriterGroupRegistryListener> _listeners;
    }
}
