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
    /// Data set item event broker - publishes locally, and also
    /// all event versions to event bus
    /// </summary>
    public sealed class PublishedDataSetEventBroker :
        IPublisherEventBroker<IPublishedDataSetListener>,
        IPublisherEvents<IPublishedDataSetListener> {

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="processor"></param>
        public PublishedDataSetEventBroker(IEventBusPublisher bus, ITaskProcessor processor = null) {
            _processor = processor;
            _listeners = new ConcurrentDictionary<string, IPublishedDataSetListener>();

            _listeners.TryAdd("v2", new Events.v2.PublishedDataSetEventBusPublisher(bus));
            // ...
        }

        /// <inheritdoc/>
        public Action Register(IPublishedDataSetListener listener) {
            var token = Guid.NewGuid().ToString();
            _listeners.TryAdd(token, listener);
            return () => _listeners.TryRemove(token, out var _);
        }

        /// <inheritdoc/>
        public Task NotifyAllAsync(Func<IPublishedDataSetListener, Task> evt) {
            Task task() => Task
                .WhenAll(_listeners.Values.Select(l => evt(l)).ToArray());
            if (_processor == null || !_processor.TrySchedule(task)) {
                return task().ContinueWith(t => Task.CompletedTask);
            }
            return Task.CompletedTask;
        }

        private readonly ITaskProcessor _processor;
        private readonly ConcurrentDictionary<string, IPublishedDataSetListener> _listeners;
    }
}
