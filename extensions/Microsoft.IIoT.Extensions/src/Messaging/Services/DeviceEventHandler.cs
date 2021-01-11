// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging.Handlers {
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Utils;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default device event handler implementation
    /// </summary>
    public sealed class DeviceEventHandler : IEventConsumer {

        /// <summary>
        /// Create processor factory
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="unknown"></param>
        public DeviceEventHandler(IEnumerable<ITelemetryHandler> handlers,
            IUnknownEventProcessor unknown = null) {
            if (handlers == null) {
                throw new ArgumentNullException(nameof(handlers));
            }
            _handlers = new ConcurrentDictionary<string, ITelemetryHandler>(
                handlers.Select(h => KeyValuePair.Create(h.MessageSchema.ToLowerInvariant(), h)));
            _unknown = unknown;
        }

        /// <inheritdoc/>
        public async Task HandleAsync(byte[] eventData,
            IEventProperties properties, Func<Task> checkpoint) {

            var handled = false;
            if (properties.TryGetValue(EventProperties.EventSchema, out var schemaType)) {
                properties.TryGetValue(EventProperties.Target, out var target);

                if (target != null &&
                    _handlers.TryGetValue(schemaType.ToLowerInvariant(), out var handler)) {
                    _used.Enqueue(handler);
                    await handler.HandleAsync(target, eventData, properties, checkpoint).ConfigureAwait(false);
                    handled = true;
                }
            }

            if (!handled && _unknown != null) {
                // From a device, but does not have any event schema or message schema
                await _unknown.HandleAsync(eventData, properties).ConfigureAwait(false);
                if (checkpoint != null) {
                    await Try.Async(() => checkpoint()).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc/>
        public async Task OnBatchCompleteAsync() {
            while (_used.TryDequeue(out var handler)) {
                await Try.Async(handler.OnBatchCompleteAsync).ConfigureAwait(false);
            }
        }

        private readonly ConcurrentQueue<ITelemetryHandler> _used =
            new ConcurrentQueue<ITelemetryHandler>();
        private readonly ConcurrentDictionary<string, ITelemetryHandler> _handlers;
        private readonly IUnknownEventProcessor _unknown;
    }
}
