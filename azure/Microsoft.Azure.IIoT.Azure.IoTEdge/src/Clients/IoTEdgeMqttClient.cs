// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.Azure.IIoT.Authentication;
    using Microsoft.Azure.IIoT.Extensions.Mqtt;
    using Microsoft.Azure.IIoT.Extensions.Mqtt.Clients;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IoT Edge mqtt client
    /// </summary>
    public sealed class IoTEdgeMqttClient : IEventPublisherClient,
        IEventSubscriberClient, IDisposable {

        /// <summary>
        /// Create mqtt client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="identity"></param>
        /// <param name="tokens"></param>
        /// <param name="edge"></param>
        /// <param name="logger"></param>
        public IoTEdgeMqttClient(ISasTokenGenerator tokens, IIdentity identity,
            IOptions<IoTEdgeClientOptions> edge, IOptions<IoTEdgeMqttOptions> options,
            ILogger logger) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _edge = edge ?? throw new ArgumentNullException(nameof(edge));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = CreateMqttClientAsync();
        }

        /// <inhertidoc/>
        public void Publish<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
            GetMqttClientAsync().ContinueWith(client => {
                if (client.IsFaulted) {
                    complete(token, client.Exception);
                }
                else if (client.IsCanceled) {
                    complete(token, new TaskCanceledException());
                }
                else {
                    client.Result.Item2.Publish(target, payload, token,
                        complete, properties, partitionKey);
                }
            }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        /// <inhertidoc/>
        public async Task PublishAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            var client = await GetMqttClientAsync().ConfigureAwait(false);
            await client.Item2.PublishAsync(target, payload, properties, partitionKey,
                ct).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public async Task PublishAsync(string target, IEnumerable<byte[]> batch,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            var client = await GetMqttClientAsync().ConfigureAwait(false);
            await client.Item2.PublishAsync(target, batch, properties, partitionKey,
                ct).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public async Task<IAsyncDisposable> SubscribeAsync(string target,
            IEventConsumer consumer) {
            var client = await GetMqttClientAsync().ConfigureAwait(false);
            return await client.Item2.SubscribeAsync(target, consumer).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public void Dispose() {
            lock (_lock) {
                if (!_client.IsFaulted && !_client.IsCanceled) {
                    _client.Result.Item2.Dispose();
                }
            }
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        private Task<(DateTime, MqttClient)> GetMqttClientAsync() {
            lock (_lock) {
                if (_client.IsFaulted || _client.IsCanceled|| (
                    _client.IsCompletedSuccessfully &&
                    _client.Result.Item1 >= DateTime.UtcNow)) {
                    _client = CreateMqttClientAsync();
                }
                return _client;
            }
        }

        /// <summary>
        /// Create mqtt client
        /// </summary>
        /// <returns></returns>
        private async Task<(DateTime, MqttClient)> CreateMqttClientAsync(
            CancellationToken ct = default) {
            var token = await _tokens.GenerateTokenAsync(_identity.Hub,
                ct).ConfigureAwait(false);
            var sas = SasToken.Parse(token);
            var expires = sas.ExpiresOn - TimeSpan.FromSeconds(30);
            var clientId = _identity.DeviceId;
            if (!string.IsNullOrEmpty(_identity.ModuleId)) {
                clientId += "/" + _identity.ModuleId;
            }
            var userName = $"{_identity.Hub}/{clientId}/?api-version=2018-06-30";
            var mqttOptions = Options.Create(new MqttOptions {
                AllowUntrustedCertificates = _edge.Value.BypassCertVerification,
                ClientId = clientId,
                UserName = userName,
                Password = sas.Signature,
                UseTls = true,
                HostName = _identity.Gateway ?? _identity.Hub,
                Port = 8883,
                QoS = _options.Value.QoS,
                QueueSize = _options.Value.QueueSize,
                Retain = _options.Value.Retain
            });
            var client = new MqttClient(mqttOptions, _logger);
            _logger.LogInformation(
                "New Mqtt broker client created. Expires {expiration}", expires);
            return (expires, client);
        }

        private Task<(DateTime, MqttClient)> _client;
        private readonly object _lock = new object();
        private readonly IOptions<IoTEdgeMqttOptions> _options;
        private readonly IOptions<IoTEdgeClientOptions> _edge;
        private readonly IIdentity _identity;
        private readonly ISasTokenGenerator _tokens;
        private readonly ILogger _logger;
    }
}
