// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.IoTEdge.Clients {
    using Microsoft.IIoT.Authentication;
    using Microsoft.IIoT.Extensions.Mqtt;
    using Microsoft.IIoT.Extensions.Mqtt.Clients;
    using Microsoft.IIoT.Hosting;
    using Microsoft.IIoT.Messaging;
    using Microsoft.IIoT.Utils;
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
            _expires = DateTime.UtcNow;
        }

        /// <inhertidoc/>
        public void Publish<T>(string target, byte[] payload, T token,
            Action<T, Exception> complete, IDictionary<string, string> properties,
            string partitionKey) {
            GetClientAsync().ContinueWith(client => {
                if (client.IsFaulted) {
                    complete(token, client.Exception);
                }
                else if (client.IsCanceled) {
                    complete(token, new TaskCanceledException());
                }
                else {
                    client.Result.Publish(target, payload, token,
                        complete, properties, partitionKey);
                }
            }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        /// <inhertidoc/>
        public async Task PublishAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            var client = await GetClientAsync(ct).ConfigureAwait(false);
            await client.PublishAsync(target, payload, properties, partitionKey,
                ct).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public async Task PublishAsync(string target, IEnumerable<byte[]> batch,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            var client = await GetClientAsync(ct).ConfigureAwait(false);
            await client.PublishAsync(target, batch, properties, partitionKey,
                ct).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public async Task<IAsyncDisposable> SubscribeAsync(string target,
            IEventConsumer consumer) {
            var client = await GetClientAsync().ConfigureAwait(false);
            return await client.SubscribeAsync(target, consumer).ConfigureAwait(false);
        }

        /// <inhertidoc/>
        public void Dispose() {
            _expires = DateTime.MaxValue;
            _client?.Dispose();
            _client = null;
            _lock.Dispose();
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        private async Task<MqttClient> GetClientAsync(CancellationToken ct = default) {
            if (_expires > DateTime.UtcNow) {
                return _client;
            }

            await _lock.WaitAsync(ct).ConfigureAwait(false); // Guard the expiration
            try {
                if (_expires > DateTime.UtcNow) {
                    return _client;
                }

                var token = await _tokens.GenerateTokenAsync(_identity.Hub,
                    ct).ConfigureAwait(false);
                var clientId = _identity.DeviceId;
                if (!string.IsNullOrEmpty(_identity.ModuleId)) {
                    clientId += "/" + _identity.ModuleId;
                }

                var userName = $"{_identity.Hub}/{clientId}/?api-version=2018-06-30";

                var mqttOptions = Options.Create(new MqttOptions {
                    AllowUntrustedCertificates = _edge.Value.BypassCertVerification,
                    ClientId = clientId,
                    UserName = userName,
                    Password = token,
                    UseTls = true,
                    HostName = _identity.Gateway ?? _identity.Hub,
                    Port = 8883,
                    QoS = _options.Value.QoS,
                    QueueSize = _options.Value.QueueSize,
                    Retain = _options.Value.Retain
                });

                _client?.Dispose();
                _client = new MqttClient(mqttOptions, _logger);
                try {
                    // Wait until successfully connected
                    await _client.ConnectAsync(ct).ConfigureAwait(false);

                    _expires = SasToken.Parse(token).ExpiresOn - TimeSpan.FromSeconds(30);
                    _logger.LogInformation(
                        "New Mqtt broker client created. Expires {expiration}", _expires);
                    return _client;
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Failed to create Mqtt broker client");
                    _client.Dispose();
                    _client = null;
                    throw;
                }
            }
            finally {
                _lock.Release();
            }
        }

        private DateTime _expires;
        private MqttClient _client;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IOptions<IoTEdgeMqttOptions> _options;
        private readonly IOptions<IoTEdgeClientOptions> _edge;
        private readonly IIdentity _identity;
        private readonly ISasTokenGenerator _tokens;
        private readonly ILogger _logger;
    }
}
