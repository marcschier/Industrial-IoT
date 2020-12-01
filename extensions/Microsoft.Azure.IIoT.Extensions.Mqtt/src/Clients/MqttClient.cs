// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.Mqtt.Clients {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Extensions.Mqtt;
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MQTTnet;
    using MQTTnet.Client.Connecting;
    using MQTTnet.Client.Disconnecting;
    using MQTTnet.Client.Options;
    using MQTTnet.Extensions.ManagedClient;
    using MQTTnet.Packets;
    using MQTTnet.Protocol;
    using MQTTnet.Server;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Mqtt event client
    /// </summary>
    public sealed class MqttClient : IEventPublisherClient, IEventSubscriberClient,
        IDisposable {

        /// <summary>
        /// Create service client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MqttClient(IOptions<MqttOptions> options, ILogger logger) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = CreateAsync();
            _queueFree.Set();
        }

        /// <inheritdoc/>
        public async Task PublishAsync(string target, byte[] payload,
            IDictionary<string, string> properties, string partitionKey,
            CancellationToken ct) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (payload == null) {
                throw new ArgumentNullException(nameof(payload));
            }
            var userProperties = properties?
                .Select(p => new MqttUserProperty(p.Key, p.Value))
                .ToList();
            var client = await GetPublishClientAsync().ConfigureAwait(false);
            string contentType = null;
            properties?.TryGetValue(EventProperties.ContentType, out contentType);
            await client.PublishAsync(new MqttApplicationMessage {
                Topic = target,
                Payload = payload,
                ContentType = contentType,
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_options.Value.QoS,
                Retain = _options.Value.Retain,
                UserProperties = userProperties
            }).ConfigureAwait(false);
            IncrementSendCount();
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
            var userProperties = properties?
                .Select(p => new MqttUserProperty(p.Key, p.Value))
                .ToList();
            var client = await GetPublishClientAsync().ConfigureAwait(false);
            string contentType = null;
            properties?.TryGetValue(EventProperties.ContentType, out contentType);
            await client.PublishAsync(batch.Select(payload => new MqttApplicationMessage {
                Topic = target,
                Payload = payload,
                ContentType = contentType,
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_options.Value.QoS,
                Retain = _options.Value.Retain,
                UserProperties = userProperties
            })).ConfigureAwait(false);
            IncrementSendCount();
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
            var t = PublishAsync(target, payload, properties, partitionKey, default)
                .ContinueWith(task => complete?.Invoke(token, task.Exception));
            t.Wait();
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeAsync(string target,
            IEventConsumer consumer) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_topics.TryGetValue(target, out var consumers)) {
                    consumers = new List<IEventConsumer> { consumer };
                    var client = await GetSubscribeClientAsync().ConfigureAwait(false);
                    await client.SubscribeAsync(new MqttTopicFilter {
                        Topic = target,
                        QualityOfServiceLevel = (MqttQualityOfServiceLevel)_options.Value.QoS
                    }).ConfigureAwait(false);
                    _topics.Add(target, consumers);
                }
                else {
                    consumers.Add(consumer);
                }
                return new AsyncDisposable(() => UnsubscribeAsync(target, consumer));
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public Task ConnectAsync(CancellationToken ct) {
            ct.Register(() => _firstConnect.TrySetCanceled());
            return _firstConnect.Task;
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (!_client.IsFaulted && !_client.IsCanceled) {
                _client.Result.Dispose();
            }
        }

        /// <summary>
        /// Handle connection
        /// </summary>
        /// <param name="args"></param>
        private void HandleClientConnected(MqttClientConnectedEventArgs args) {
            _connected.Set();
            _firstConnect.TrySetResult(true);
            _logger.LogInformation("Client connected with {result}.",
                args.AuthenticateResult.ResultCode);
        }

        /// <summary>
        /// Handle connection failure
        /// </summary>
        /// <param name="args"></param>
        private void HandleClientConnectingFailed(ManagedProcessFailedEventArgs args) {
            _connected.Reset();
            _firstConnect.TrySetException(args.Exception);
            _logger.LogError(args.Exception, "Client connecting failed.");
        }

        /// <summary>
        /// Handle subscription failed
        /// </summary>
        /// <param name="args"></param>
        private void HandleSubscriptionFailed(ManagedProcessFailedEventArgs args) {
            _firstConnect.TrySetException(args.Exception);
            _logger.LogError(args.Exception, "Subscription synchronization failed.");
        }

        /// <summary>
        /// Handle message skipped
        /// </summary>
        /// <param name="args"></param>
        private void HandleMessageSkipped(ApplicationMessageSkippedEventArgs args) {
            _logger.LogWarning("Message {id} for {topic} was skipped.",
                args.ApplicationMessage.Id, args.ApplicationMessage.ApplicationMessage.Topic);
            DecrementSendCount();
        }

        /// <summary>
        /// Handle message processed
        /// </summary>
        /// <param name="args"></param>
        private void HandleMessageProcessed(ApplicationMessageProcessedEventArgs args) {
            _logger.LogTrace("Message {id} for {topic} was processed",
                args.ApplicationMessage.Id, args.ApplicationMessage.ApplicationMessage.Topic);
            DecrementSendCount();
        }

        /// <summary>
        /// Handle message receival
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args) {
            if (args?.ApplicationMessage == null) {
                return;
            }
            var topic = args.ApplicationMessage.Topic;
            _logger.LogTrace("Client received message from {client} on {topic}",
                args.ClientId, topic);
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_topics.TryGetValue(topic, out var consumers)) {
                    _logger.LogInformation("Topic {topic} not subscribed to - dropping.",
                        topic);
                    return;
                }
                var userProperties = args.ApplicationMessage.UserProperties?
                    .ToDictionary(u => u.Name, u => u.Value) ??
                        new Dictionary<string, string>();
                foreach (var consumer in consumers) {
                    await consumer.HandleAsync(args.ApplicationMessage.Payload,
                        userProperties, () => Task.CompletedTask).ConfigureAwait(false);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Handle disconnected
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private void HandleClientDisconnected(MqttClientDisconnectedEventArgs args) {
            _connected.Reset();
            if (args.Exception != null) {
                _firstConnect.TrySetException(args.Exception);
                _logger.LogError(args.Exception, "Disconnected {state} due to {reason}",
                    args.ClientWasConnected ? "connecting" : "connected", args.ReasonCode);
            }
            else {
                _logger.LogInformation("{state} client disconnected due to {reason}",
                    args.ClientWasConnected ? "Connecting" : "Connected", args.ReasonCode);
            }
        }

        /// <summary>
        /// Remove subscription and unsubscribe if needed
        /// </summary>
        /// <param name="target"></param>
        /// <param name="consumer"></param>
        /// <returns></returns>
        private async Task UnsubscribeAsync(string target, IEventConsumer consumer) {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_topics.TryGetValue(target, out var consumers)) {
                    throw new ResourceInvalidStateException("Subscription not found");
                }
                consumers.Remove(consumer);
                if (consumers.Count == 0) {
                    var client = await _client.ConfigureAwait(false);
                    await client.UnsubscribeAsync(target).ConfigureAwait(false);
                    _topics.Remove(target);
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Increment sent count for flow control
        /// </summary>
        private void IncrementSendCount() {
            if (_options.Value.QueueSize == null) {
                return;
            }
            if (Interlocked.Increment(ref _queueSize) > _options.Value.QueueSize) {
                _queueFree.Reset();
            }
        }

        /// <summary>
        /// Decrement send count for flow control
        /// </summary>
        private void DecrementSendCount() {
            if (_options.Value.QueueSize == null) {
                return;
            }
            if (Interlocked.Decrement(ref _queueSize) < _options.Value.QueueSize / 2) {
                _queueFree.Set();
            }
        }

        /// <summary>
        /// Get publisher client
        /// </summary>
        /// <returns></returns>
        private async Task<IManagedMqttClient> GetPublishClientAsync() {
            var client = await _client.ConfigureAwait(false);
            // Wait until connected
            await _connected.WaitAsync().ConfigureAwait(false);
            // Wait until queue drained
            await _queueFree.WaitAsync().ConfigureAwait(false);
            return client;
        }

        /// <summary>
        /// Gets subscriber client
        /// </summary>
        /// <returns></returns>
        private async Task<IManagedMqttClient> GetSubscribeClientAsync() {
            return await _client.ConfigureAwait(false);
        }

        /// <summary>ping
        /// Create client and start it
        /// </summary>
        /// <returns></returns>
        public async Task<IManagedMqttClient> CreateAsync() {
            var client = new MqttFactory().CreateManagedMqttClient()
                .UseApplicationMessageReceivedHandler(HandleMessageReceivedAsync)
                .UseConnectedHandler(HandleClientConnected)
                .UseDisconnectedHandler(HandleClientDisconnected)
                ;
            client.ConnectingFailedHandler =
                new ConnectingFailedHandlerDelegate(HandleClientConnectingFailed);
            client.ApplicationMessageProcessedHandler =
                new ApplicationMessageProcessedHandlerDelegate(HandleMessageProcessed);
            client.ApplicationMessageSkippedHandler =
                new ApplicationMessageSkippedHandlerDelegate(HandleMessageSkipped);
            client.SynchronizingSubscriptionsFailedHandler =
                new SynchronizingSubscriptionsFailedHandlerDelegate(HandleSubscriptionFailed);
            var options = new ManagedMqttClientOptions {
                AutoReconnect = true,
                ClientOptions = new MqttClientOptions {
                    ClientId = _options.Value.ClientId,
                    Credentials = _options.Value.UserName == null ? null :
                            new MqttClientCredentials {
                                Username = _options.Value.UserName,
                                Password = _options.Value.Password == null ? null :
                                    Encoding.UTF8.GetBytes(_options.Value.Password)
                            },
                    ChannelOptions = new MqttClientTcpOptions {
                        Server = _options.Value.HostName,
                        Port = _options.Value.Port,
                        TlsOptions = new MqttClientTlsOptions {
                            CertificateValidationHandler = context => {
                                if (_options.Value.AllowUntrustedCertificates ?? false) {
                                    return true;
                                }
                                return context.SslPolicyErrors == SslPolicyErrors.None;
                            },
                            AllowUntrustedCertificates =
                                    _options.Value.AllowUntrustedCertificates ?? false,
                            UseTls = _options.Value.UseTls ?? true,
                        }
                    }
                    // ...
                },
                PendingMessagesOverflowStrategy =
                        MqttPendingMessagesOverflowStrategy.DropNewMessage,
                MaxPendingMessages = _options.Value.QueueSize == null ? int.MaxValue :
                        (int)Math.Max(int.MaxValue, _options.Value.QueueSize.Value * 2),
                Storage = null // todo
            };
            try {
                await client.StartAsync(options).ConfigureAwait(false);
                return client;
            }
            catch {
                client.Dispose();
                throw;
            }
        }

        private volatile int _queueSize;
        private readonly TaskCompletionSource<bool> _firstConnect =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly IOptions<MqttOptions> _options;
        private readonly ILogger _logger;
        private readonly Task<IManagedMqttClient> _client;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly AsyncManualResetEvent _connected = new();
        private readonly AsyncManualResetEvent _queueFree = new();
        private readonly Dictionary<string, List<IEventConsumer>> _topics = new();
    }
}
