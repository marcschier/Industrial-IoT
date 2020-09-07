﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.Kafka.Handlers {
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default Kafka device event handler implementation
    /// </summary>
    public sealed class KafkaDeviceEventHandler : IEventProcessingHandler {

        /// <summary>
        /// Create processor factory
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="unknown"></param>
        public KafkaDeviceEventHandler(IEnumerable<ITelemetryHandler> handlers,
            IUnknownEventProcessor unknown = null) {
            if (handlers == null) {
                throw new ArgumentNullException(nameof(handlers));
            }
            _handlers = new ConcurrentDictionary<string, ITelemetryHandler>(
                handlers.Select(h => KeyValuePair.Create(h.MessageSchema.ToLowerInvariant(), h)));
            _unknown = unknown;
        }

        /// <inheritdoc/>
        public async Task HandleAsync(byte[] eventData, IDictionary<string, string> properties,
            Func<Task> checkpoint) {

            var handled = false;
            if (properties.TryGetValue(CommonProperties.EventSchemaType, out var schemaType)) {

                properties.TryGetValue(CommonProperties.DeviceId, out var deviceId);
                properties.TryGetValue(CommonProperties.ModuleId, out var moduleId);

                if (_handlers.TryGetValue(schemaType, out var handler)) {
                    _used.Enqueue(handler);
                    await handler.HandleAsync(deviceId, moduleId, eventData, properties, checkpoint);
                    handled = true;
                }
            }

            if (!handled && _unknown != null) {
                // From a device, but does not have any event schema or message schema
                await _unknown.HandleAsync(eventData, properties);
                if (checkpoint != null) {
                    await Try.Async(() => checkpoint());
                }
            }
        }

        /// <inheritdoc/>
        public async Task OnBatchCompleteAsync() {
            while (_used.TryDequeue(out var handler)) {
                await Try.Async(handler.OnBatchCompleteAsync);
            }
        }

        private readonly ConcurrentQueue<ITelemetryHandler> _used =
            new ConcurrentQueue<ITelemetryHandler>();
        private readonly ConcurrentDictionary<string, ITelemetryHandler> _handlers;
        private readonly IUnknownEventProcessor _unknown;
    }
}
