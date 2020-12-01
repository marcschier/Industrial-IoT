// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTHub.Handlers {
    using Microsoft.Azure.IIoT.Azure.IoTHub;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default iot hub device event handler implementation
    /// </summary>
    public sealed class IoTHubDeviceEventHandler : IEventConsumer {

        /// <summary>
        /// Create processor factory
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="unknown"></param>
        public IoTHubDeviceEventHandler(IEnumerable<ITelemetryHandler> handlers,
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
            IDictionary<string, string> properties, Func<Task> checkpoint) {
            if (!properties.TryGetValue(SystemProperties.ConnectionDeviceId, out var deviceId) &&
                !properties.TryGetValue(SystemProperties.DeviceId, out deviceId)) {
                // Not from a device
                return;
            }

            if (!properties.TryGetValue(SystemProperties.ConnectionModuleId, out var moduleId) &&
                !properties.TryGetValue(SystemProperties.ModuleId, out moduleId)) {
                // Not from a module
                moduleId = null;
            }

            if (properties.TryGetValue(EventProperties.EventSchema, out var schemaType) ||
                properties.TryGetValue(SystemProperties.MessageSchema, out schemaType)) {

                //  TODO: when handling third party OPC UA PubSub Messages
                //  the schemaType might not exist
                var source = HubResource.Format(null, deviceId, moduleId);
                if (_handlers.TryGetValue(schemaType.ToLowerInvariant(), out var handler)) {
                    await handler.HandleAsync(source, eventData, properties, checkpoint).ConfigureAwait(false);
                    _used.Enqueue(handler);
                }

                // Handled...
                return;
            }

            if (_unknown != null) {
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
