// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Extensions.RabbitMq.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RabbitMQ.Client;

    /// <summary>
    /// Implements queue consumer host for rabbit mq
    /// </summary>
    public sealed class RabbitMqConsumerHost : IEventProcessingHost,
        IRabbitMqConsumer, IDisposable {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="logger"></param>
        /// <param name="consumer"></param>
        /// <param name="options"></param>
        public RabbitMqConsumerHost(IRabbitMqConnection connection, ILogger logger, 
            IEventProcessingHandler consumer, IOptionsSnapshot<RabbitMqQueueOptions> options) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _lock = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_channel != null) {
                    throw new ResourceInvalidStateException("Already started");
                }
                try {
                    var queue = _options.Value.Queue.Split('/')[0];
                    _channel = _connection.GetChannel(_options.Value.Queue, this);
                    _logger.LogInformation("Queue {queue} processor started.",
                        _channel.QueueName);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Failure starting queue processor.");
                    throw;
                }
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_channel == null) {
                    return;
                }
                _channel.Dispose();
                _logger.LogInformation("Queue {queue} processor stopped.",
                    _channel.QueueName);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failure stopping queue processor.");
                throw;
            }
            finally {
                _channel = null;
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
            _channel?.Dispose();
            _lock?.Dispose();
        }

        /// <summary>
        /// Call handler
        /// </summary>
        /// <param name="model"></param>
        /// <param name="deliveryTag"></param>
        /// <param name="redelivered"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="properties"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task HandleBasicDeliver(IModel model, ulong deliveryTag,
            bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, ReadOnlyMemory<byte> body) {

            await _consumer.HandleAsync(body.ToArray(),
                properties.ToDictionary(deliveryTag),
                () => Task.CompletedTask).ConfigureAwait(false);
            await Try.Async(_consumer.OnBatchCompleteAsync).ConfigureAwait(false);
        }

        private readonly ILogger _logger;
        private readonly IRabbitMqConnection _connection;
        private readonly IEventProcessingHandler _consumer;
        private readonly IOptionsSnapshot<RabbitMqQueueOptions> _options;
        private readonly SemaphoreSlim _lock;
        private IRabbitMqChannel _channel;
    }
}
