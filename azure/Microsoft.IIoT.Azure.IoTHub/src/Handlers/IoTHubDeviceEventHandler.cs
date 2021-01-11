// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTHub.Handlers {
    using Microsoft.IIoT.Azure.IoTHub;
    using Microsoft.IIoT.Extensions.Messaging;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.IIoT.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections;

    /// <summary>
    /// Default iot hub device event handler implementation
    /// </summary>
    public sealed class IoTHubDeviceEventHandler : IEventConsumer {

        /// <summary>
        /// Create processor factory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="handlers"></param>
        /// <param name="unknown"></param>
        public IoTHubDeviceEventHandler(IOptions<IoTHubServiceOptions> options,
            IEnumerable<ITelemetryHandler> handlers, IUnknownEventProcessor unknown = null) {
            _hostName = ConnectionString.Parse(options.Value.ConnectionString).HostName;
            if (handlers == null) {
                throw new ArgumentNullException(nameof(handlers));
            }
            _handlers = new ConcurrentDictionary<string, ITelemetryHandler>(
                handlers.Select(h => KeyValuePair.Create(h.MessageSchema.ToLowerInvariant(), h)));
            _unknown = unknown;
        }

        /// <inheritdoc/>
        public async Task HandleAsync(byte[] eventData,
            IEventProperties properties2, Func<Task> checkpoint) {

            var properties = new IoTHubProperties(properties2, _hostName);
            if (properties.EventSchema != null) {
                if (properties.Target != null) {
                    if (_handlers.TryGetValue(properties.EventSchema.ToLowerInvariant(), out var handler)) {
                        await handler.HandleAsync(properties.Target, eventData, properties,
                            checkpoint).ConfigureAwait(false);
                        _used.Enqueue(handler);
                    }
                }
                // Handled or without target...
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

        /// <summary>
        /// Wraps the properties into a string dictionary
        /// </summary>
        private class IoTHubProperties : IEventProperties {

            /// <inheritdoc/>
            public string ContentEncoding {
                get {
                    if (_properties.TryGetValue(EventProperties.ContentEncoding, out var encoding) ||
                        _properties.TryGetValue(SystemProperties.ContentEncoding, out encoding)) {
                        return encoding;
                    }
                    return null;
                }
            }

            /// <inheritdoc/>
            public string ContentType {
                get {
                    if (_properties.TryGetValue(EventProperties.ContentType, out var contentType) ||
                        _properties.TryGetValue(SystemProperties.ContentType, out contentType)) {
                        return contentType;
                    }
                    return null;
                }
            }

            /// <inheritdoc/>
            public string Target {
                get {

                    if (!_properties.TryGetValue(SystemProperties.ConnectionDeviceId, out var deviceId) &&
                        !_properties.TryGetValue(SystemProperties.DeviceId, out deviceId)) {
                        // Not from a device
                        return null;
                    }

                    if (!_properties.TryGetValue(SystemProperties.ConnectionModuleId, out var moduleId) &&
                        !_properties.TryGetValue(SystemProperties.ModuleId, out moduleId)) {
                        // Not from a module
                        moduleId = null;
                    }

                    if (!_properties.TryGetValue(SystemProperties.HubName, out var hubName)) {
                        hubName = _hubName;
                    }
                    return HubResource.Format(hubName, deviceId, moduleId);
                }
            }

            /// <inheritdoc/>
            public string EventSchema {
                get {
                    if (_properties.TryGetValue(EventProperties.EventSchema, out var schemaType) ||
                        _properties.TryGetValue(SystemProperties.MessageSchema, out schemaType)) {
                        return schemaType;
                    }
                    return null;
                }
            }

            /// <inheritdoc/>
            public string this[string key] {
                get {
                    TryGetValue(key, out var value);
                    return value;
                }
            }

            /// <summary>
            /// Create properties wrapper
            /// </summary>
            /// <param name="system"></param>
            /// <param name="hubName"></param>
            internal IoTHubProperties(IEventProperties system, string hubName) {
                _properties = system;
                _hubName = hubName;
            }

            /// <inheritdoc/>
            public bool TryGetValue(string key, out string value) {
                if (!_properties.TryGetValue(key, out value)) {
                    switch (key) {
                        case EventProperties.ContentEncoding:
                            value = ContentEncoding;
                            break;
                        case EventProperties.ContentType:
                            value = ContentType;
                            break;
                        case EventProperties.Target:
                            value = Target;
                            break;
                        case EventProperties.EventSchema:
                            value = EventSchema;
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
                return _properties.Concat(GetValues()).GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() {
                return ((IEnumerable)_properties.Concat(GetValues())).GetEnumerator();
            }

            /// <summary>
            /// Helper to yield all adapted values if not already in core properties
            /// </summary>
            /// <returns></returns>
            private IEnumerable<KeyValuePair<string, string>> GetValues() {
                if (!_properties.TryGetValue(EventProperties.ContentEncoding, out _)) {
                    yield return KeyValuePair.Create(EventProperties.ContentEncoding, ContentEncoding);
                }
                if (!_properties.TryGetValue(EventProperties.ContentType, out _)) {
                    yield return KeyValuePair.Create(EventProperties.ContentType, ContentType);
                }
                if (!_properties.TryGetValue(EventProperties.Target, out _)) {
                    yield return KeyValuePair.Create(EventProperties.Target, Target);
                }
                if (!_properties.TryGetValue(EventProperties.EventSchema, out _)) {
                    yield return KeyValuePair.Create(EventProperties.EventSchema, EventSchema);
                }
            }

            private readonly IEventProperties _properties;
            private readonly string _hubName;
        }

        private readonly ConcurrentQueue<ITelemetryHandler> _used =
            new ConcurrentQueue<ITelemetryHandler>();
        private readonly ConcurrentDictionary<string, ITelemetryHandler> _handlers;
        private readonly IUnknownEventProcessor _unknown;
        private readonly string _hostName;
    }
}
