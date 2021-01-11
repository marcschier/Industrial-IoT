// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.RabbitMq.Clients {
    using Microsoft.IIoT.Exceptions;
    using Microsoft.IIoT.Extensions.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Exceptions;

    /// <summary>
    /// Rabbitmq connection
    /// </summary>
    public sealed class RabbitMqConnection : IRabbitMqConnection, IDisposable {

        /// <summary>
        /// Create connection
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public RabbitMqConnection(IOptionsSnapshot<RabbitMqOptions> options, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _connection = Retry.WithLinearBackoff(_logger, () =>
                Task.FromResult(new ConnectionFactory {
                    HostName = _options.Value.HostName,
                    Password = _options.Value.Key,
                    UserName = _options.Value.UserName,

                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromMilliseconds(500),
                    DispatchConsumersAsync = true
                }.CreateConnection()), ex => ex is BrokerUnreachableException, 60).Result;
        }

        /// <inheritdoc/>
        public IRabbitMqChannel GetChannel(string name, IRabbitMqConsumer consumer,
            bool fanout) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException(nameof(name));
            }
            return new RabbitMqChannel(this, name, fanout, consumer);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _connection?.Dispose();
        }

        /// <summary>
        /// Channel
        /// </summary>
        private class RabbitMqChannel : IRabbitMqChannel {

            /// <inheritdoc/>
            public string QueueName { get; private set; }

            /// <inheritdoc/>
            public string ExchangeName { get; private set; }

            /// <inheritdoc/>
            public string RoutingKey {
                get {
                    if (_consumer != null || string.IsNullOrEmpty(QueueName)) {
                        return _outer._options.Value.RoutingKey ?? string.Empty;
                    }
                    return QueueName;
                }
            }

            /// <summary>
            /// Create channel
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="name"></param>
            /// <param name="pubSub"></param>
            /// <param name="consumer"></param>
            internal RabbitMqChannel(RabbitMqConnection outer, string name,
                bool pubSub, IRabbitMqConsumer consumer) {
                _logger = outer._logger;
                _scope = _logger.BeginScope("SourceContext", new {
                    ChannelName = name
                });
                _outer = outer;
                _name = name;
                _pubSub = pubSub;
                _consumer = consumer;
                _channel = CreateChannel();
            }

            /// <inheritdoc/>
            public void Publish<T>(ReadOnlyMemory<byte> body, T token,
                Action<T, Exception> complete, Action<IBasicProperties> props,
                bool mandatory) {

                if (_consumer != null) {
                    throw new NotSupportedException("Consumer channel");
                }

                lock (_channel) {
                    var seq = _channel.Model.NextPublishSeqNo;
                    if (!_completions.TryAdd(seq, ex => complete(token, ex))) {
                        throw new ResourceInvalidStateException(
                            "sequence number in use");
                    };

                    var properties = _channel.Model.CreateBasicProperties();
                    props?.Invoke(properties);
                    _channel.Model.BasicPublish(ExchangeName, RoutingKey,
                            mandatory, properties, body);
                }
            }

            /// <inheritdoc/>
            public void Publish<T>(IEnumerable<ReadOnlyMemory<byte>> batch,
                T token, Action<T, Exception> complete, Action<IBasicProperties> props,
                bool mandatory) {

                if (_consumer != null) {
                    throw new NotSupportedException("Consumer channel");
                }

                lock (_channel) {
                    var items = batch.ToList();
                    var lastSeq = _channel.Model.NextPublishSeqNo + (ulong)items.Count - 1;
                    if (!_completions.TryAdd(lastSeq, ex => complete(token, ex))) {
                        throw new ResourceInvalidStateException(
                            "sequence number in use");
                    };
                    var bulk = _channel.Model.CreateBasicPublishBatch();
                    foreach (var body in items) {
                        var properties = _channel.Model.CreateBasicProperties();
                        props?.Invoke(properties);
                        bulk.Add(ExchangeName, RoutingKey, mandatory, properties, body);
                    }
                    bulk.Publish();
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                CloseChannel();
                _isDisposed = true;
                _scope.Dispose();
            }

            /// <summary>
            /// Create model
            /// </summary>
            /// <returns></returns>
            private Channel CreateChannel() {
                var attempt = 1;
                while (true) {
                    try {
                        return CreateChannelInternal();
                    }
                    catch (OperationInterruptedException ex) {
                        _logger.LogError(ex, "Failed to open channel {attempt}", attempt);
                        if (++attempt > 10) {
                            throw;
                        }
                        // Try again
                    }
                }
            }

            /// <summary>
            /// Create channel
            /// </summary>
            /// <returns></returns>
            private Channel CreateChannelInternal() {
                if (_isDisposed) {
                    throw new ObjectDisposedException(nameof(RabbitMqChannel));
                }

                var model = _outer._connection.CreateModel();

                if (_consumer == null) {
                    // Publisher queue
                    if (_pubSub) {
                        // Create exchange
                        QueueName = string.Empty; // default
                        ExchangeName = _name;

                        model.ExchangeDeclare(ExchangeName,
                            string.IsNullOrEmpty(RoutingKey) ?
                                ExchangeType.Fanout : ExchangeType.Direct, true);
                    }
                    else {
                        // Create Queue
                        QueueName = model.QueueDeclare(_name, true, false).QueueName;
                        ExchangeName = _outer._options.Value.RoutingKey ?? string.Empty;
                        if (!string.IsNullOrEmpty(ExchangeName)) {
                            model.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
                        }
                    }

                    model.ConfirmSelect();
                    model.BasicAcks += (sender, ea) =>
                        HandleConfirm(ea.Multiple, ea.DeliveryTag, () => null);
                    model.BasicNacks += (sender, ea) =>
                        HandleConfirm(ea.Multiple, ea.DeliveryTag,
                            () => new CommunicationException("Failed sending"));
                }
                else {
                    // Consumer queues
                    if (_pubSub) {
                        // Create queue and exchange
                        QueueName = model.QueueDeclare().QueueName;
                        ExchangeName = _name;

                        // Create exchange and bind queue to it
                        model.ExchangeDeclare(ExchangeName,
                            string.IsNullOrEmpty(RoutingKey) ?
                                ExchangeType.Fanout : ExchangeType.Direct, true);
                        model.QueueBind(QueueName, ExchangeName, RoutingKey);
                    }
                    else {
                        // Create Queue
                        QueueName = model.QueueDeclare(_name, true, false).QueueName;
                        ExchangeName = _outer._options.Value.RoutingKey ?? string.Empty;
                        if (!string.IsNullOrEmpty(ExchangeName)) {
                            model.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
                        }
                    }

                    // Channel creation sets up consumption
                }
                return new Channel(model, this);
            }

            /// <summary>
            /// Handle confirmation
            /// </summary>
            /// <param name="multiple"></param>
            /// <param name="sequenceNumber"></param>
            /// <param name="ex"></param>
            private void HandleConfirm(bool multiple, ulong sequenceNumber,
                Func<Exception> ex) {
                if (!multiple) {
                    if (_completions.TryRemove(sequenceNumber, out var a)) {
                        Try.Op(() => a.Invoke(ex()));
                    }
                }
                else {
                    var confirmed = _completions.Where(k => k.Key <= sequenceNumber);
                    foreach (var entry in confirmed) {
                        if (_completions.TryRemove(entry.Key, out var a)) {
                            Try.Op(() => a.Invoke(ex()));
                        }
                    }
                }
            }

            /// <summary>
            /// Close channel
            /// </summary>
            private void CloseChannel() {
                if (_isDisposed) {
                    throw new ObjectDisposedException(nameof(RabbitMqChannel));
                }
                if (_channel != null) {
                    while (_consumer == null && !_completions.IsEmpty) {
                        foreach (var entry in _completions.ToList()) {
                            if (_completions.TryRemove(entry.Key, out var a)) {
                                Try.Op(() => a.Invoke(
                                    new CommunicationException("Closed")));
                            }
                        }
                    }
                    _channel.Dispose();
                }
            }

            /// <summary>
            /// Internal channel object
            /// </summary>
            private class Channel : AsyncDefaultBasicConsumer, IDisposable {

                /// <inheritdoc/>
                public Channel(IModel model, RabbitMqChannel outer) :
                    base(model) {
                    _outer = outer;
                    _logger = outer._logger;
                    _scope = _logger.BeginScope(new {
                        outer.QueueName,
                        outer.ExchangeName,
                        ConsumerTag = _consumerTag
                    });
                    if (_outer._consumer != null) {
                        // Start consume
                        model.BasicConsume(_outer.QueueName, true, this);
                    }
                }

                /// <inheritdoc/>
                public override Task HandleBasicDeliver(string consumerTag,
                    ulong deliveryTag, bool redelivered, string exchange,
                    string routingKey, IBasicProperties properties,
                    ReadOnlyMemory<byte> body) {
                    return _outer._consumer.HandleBasicDeliver(Model, deliveryTag,
                        redelivered, exchange, routingKey, properties, body);
                }

                /// <inheritdoc/>
                public override Task HandleModelShutdown(object model,
                    ShutdownEventArgs reason) {
                    _logger.LogInformation("Channel shutdown by {initiator}.",
                        reason.Initiator);
                    if (reason.Initiator == ShutdownInitiator.Peer) {
                        // TODO - restart
                    }
                    return base.HandleModelShutdown(model, reason);
                }

                /// <inheritdoc/>
                public override Task HandleBasicConsumeOk(string consumerTag) {
                    // Consuming
                    _logger.LogInformation("Starting to consume");
                    return base.HandleBasicConsumeOk(consumerTag);
                }

                /// <inheritdoc/>
                public void Dispose() {
                    try {
                        if (Model.IsClosed) {
                            return;
                        }
                        if (IsRunning) {
                            // Stop consume
                            Model.BasicCancelNoWait(_consumerTag);
                        }
                        Model.Close();
                    }
                    finally {
                        Model.Dispose();
                        _scope.Dispose();
                    }
                }

                private readonly ILogger _logger;
                private readonly IDisposable _scope;
                private readonly RabbitMqChannel _outer;
                private readonly string _consumerTag = Guid.NewGuid().ToString();
            }

            private readonly Channel _channel;
            private bool _isDisposed;
            private readonly string _name;
            private readonly bool _pubSub;
            private readonly ILogger _logger;
            private readonly IDisposable _scope;
            private readonly IRabbitMqConsumer _consumer;
            private readonly RabbitMqConnection _outer;
            private readonly ConcurrentDictionary<ulong, Action<Exception>> _completions =
                new ConcurrentDictionary<ulong, Action<Exception>>();
        }

        private readonly IConnection _connection;
        private readonly ILogger _logger;
        private readonly IOptionsSnapshot<RabbitMqOptions> _options;
    }
}
