// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Mqtt.Clients {
    using Microsoft.Azure.IIoT.Extensions.Mqtt;
    using Microsoft.Azure.IIoT.Hub;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Extensions.Options;
    using MQTTnet;
    using MQTTnet.Extensions.ManagedClient;
    using MQTTnet.Packets;
    using MQTTnet.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Mqtt event client
    /// </summary>
    public sealed class MqttEventPublisher : IEventPublisherClient, IEventClient, IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="client"></param>
        public MqttEventPublisher(IOptionsSnapshot<MqttClientOptions> config,
            IManagedMqttClient client) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public Task PublishAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            return _client.PublishAsync(new MqttApplicationMessage {
                Topic = target,
                Payload = payload,
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_config.Value.QoS,
                Retain = _config.Value.Retain,
                UserProperties = properties?
                    .Select(p => new MqttUserProperty(p.Key, p.Value))
                    .ToList()
            });
        }

        /// <inheritdoc/>
        public async Task PublishAsync(string target, IEnumerable<byte[]> batch,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            foreach (var payload in batch) {
                await PublishAsync(target, payload, properties, partitionKey,
                    ct).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Publish<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            _ = PublishAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            return _client.PublishAsync(new MqttApplicationMessage {
                Topic = target,
                Payload = payload,
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_config.Value.QoS,
                Retain = _config.Value.Retain,
                ContentType = contentType,
                UserProperties = CreateProperties(contentType, eventSchema, contentEncoding)
            });
        }

        /// <inheritdoc/>
        public Task SendEventAsync(string target, IEnumerable<byte[]> batch,
            string contentType, string eventSchema, string contentEncoding, CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (batch == null) {
                throw new ArgumentNullException(nameof(batch));
            }
            var props = CreateProperties(contentType, eventSchema, contentEncoding);
            return _client.PublishAsync(batch.Select(payload => new MqttApplicationMessage {
                Topic = target,
                Payload = payload,
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_config.Value.QoS,
                Retain = _config.Value.Retain,
                ContentType = contentType,
                UserProperties = CreateProperties(contentType, eventSchema, contentEncoding)
            }));
        }

        /// <inheritdoc/>
        public void SendEvent<T>(string target, byte[] payload, string contentType,
            string eventSchema, string contentEncoding, T token, Action<T, Exception> complete) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            if (token is null) {
                throw new ArgumentNullException(nameof(token));
            }
            if (complete == null) {
                throw new ArgumentNullException(nameof(complete));
            }
            _ = SendEventAsync(target, payload, contentType, eventSchema, contentEncoding, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
        }

        /// <inheritdoc/>
        public void Dispose() {
            _client?.Dispose();
        }

        /// <summary>
        /// Create properties
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="eventSchema"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        private static List<MqttUserProperty> CreateProperties(
            string contentType, string eventSchema, string contentEncoding) {
            var props = new List<MqttUserProperty>();
            if (!string.IsNullOrEmpty(contentType)) {
                props.Add(new MqttUserProperty(EventProperties.ContentType, contentType));
            }
            if (!string.IsNullOrEmpty(contentEncoding)) {
                props.Add(new MqttUserProperty(EventProperties.ContentEncoding, contentEncoding));
            }
            if (!string.IsNullOrEmpty(eventSchema)) {
                props.Add(new MqttUserProperty(EventProperties.EventSchema, eventSchema));
            }
            return props;
        }

        private readonly IOptionsSnapshot<MqttClientOptions> _config;
        private readonly IManagedMqttClient _client;
    }
}
