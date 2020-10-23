// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Implementation of event processor host interface handle queue messages.
    /// </summary>
    public sealed class ServiceBusProcessorHost : IEventProcessingHost, IDisposable {

        /// <summary>
        /// Create host
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="consumer"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public ServiceBusProcessorHost(IServiceBusClientFactory factory,
            IEventProcessingHandler consumer, IServiceBusProcessorConfig config,
            ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _lock = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc/>
        public async Task StartAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (_client != null) {
                    throw new ResourceInvalidStateException("Already started");
                }
                try {
                    _client = await _factory.CreateOrGetGetQueueClientAsync(_config.Queue).ConfigureAwait(false);
                    _client.RegisterMessageHandler(OnMessageAsync, OnExceptionAsync);
                    _logger.Information("Queue {queue} processor started.", _client.QueueName);
                }
                catch (Exception ex) {
                    _logger.Error(ex, "Failure starting queue processor.");
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
                if (_client == null) {
                    return;
                }
                await _client.CloseAsync().ConfigureAwait(false);
                _logger.Information("Queue {queue} processor stopped.", _client.QueueName);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failure stopping queue processor.");
                throw;
            }
            finally {
                _client = null;
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Op(() => StopAsync().Wait());
            _lock.Dispose();
        }

        /// <summary>
        /// Handle exception
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task OnExceptionAsync(ExceptionReceivedEventArgs arg) {
            _logger.Error(arg.Exception, "Exception in queue {queue} processor : {@context}.",
                 _client.QueueName, arg.ExceptionReceivedContext);

            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                _logger.Information("Resetting queue {queue} processor...", _client.QueueName);
                await _client.CloseAsync().ConfigureAwait(false);
                _client = await _factory.CreateOrGetGetQueueClientAsync(_config.Queue).ConfigureAwait(false);
                _client.RegisterMessageHandler(OnMessageAsync, OnExceptionAsync);
                _logger.Information("Queue {queue} processor reset.", _client.QueueName);
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Call handler
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task OnMessageAsync(Message ev, CancellationToken ct) {
            await _consumer.HandleAsync(ev.Body, ev.UserProperties
                .ToDictionary(k => k.Key, v => v.Value?.ToString()),
                () => Task.CompletedTask).ConfigureAwait(false);
            await Try.Async(_consumer.OnBatchCompleteAsync).ConfigureAwait(false);
        }

        private readonly ILogger _logger;
        private readonly IServiceBusClientFactory _factory;
        private readonly IEventProcessingHandler _consumer;
        private readonly IServiceBusProcessorConfig _config;
        private readonly SemaphoreSlim _lock;
        private IQueueClient _client;
    }
}
