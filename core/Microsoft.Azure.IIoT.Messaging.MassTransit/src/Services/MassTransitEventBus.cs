// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging.MassTransit.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Serilog;
    using GreenPipes;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using global::MassTransit;
    using global::MassTransit.Pipeline;
    using Microsoft.Azure.IIoT.Exceptions;

    /// <summary>
    /// Event bus built on top of mass transit
    /// </summary>
    public class MassTransitEventBus : IEventBus, IDisposable {

        /// <summary>
        /// Create mass transit bus event bus
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="registration"></param>
        /// <param name="connector"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public MassTransitEventBus(IPublishEndpointProvider publisher, IRegistration registration,
            IConsumePipeConnector connector, IReceiveEndpointConfigurator config, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _registration = registration ?? throw new ArgumentNullException(nameof(registration));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Create mass transit bus event bus
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="registration"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public MassTransitEventBus(IBus bus, IRegistration registration,
            IReceiveEndpointConfigurator config, ILogger logger) :
            this (bus, registration, bus, config, logger) {
        }

        /// <inheritdoc/>
        public async Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            try {
                var endpoint = await _publisher.GetPublishSendEndpoint<Event<T>>();
                await endpoint.Send(new Event<T> {
                    Value = message
                }).ConfigureAwait(false);
                _logger.Verbose("----->  {@message} sent...", message);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to publish message {@message}", message);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task CloseAsync() {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose() {
            foreach (var handle in _handlers.Values.ToList()) {
                handle.Disconnect();
            }
            _handlers.Clear();
        }

        /// <inheritdoc/>
        public Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            _config.ConfigureConsumer<Consumer<T>>(_registration);
            var handle = _connector.ConnectConsumer(() => new Consumer<T>(this, handler));
            var token = Guid.NewGuid().ToString();
            _handlers.TryAdd(token, handle);
            return Task.FromResult(token);
        }

        /// <inheritdoc/>
        public Task UnregisterAsync(string token) {
            if (string.IsNullOrEmpty(token)) {
                throw new ArgumentNullException(nameof(token));
            }
            if (!_handlers.TryRemove(token, out var handle)) {
                throw new ResourceInvalidStateException("Token not found");
            }
            handle.Disconnect();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Consumer wrapper
        /// </summary>
        internal class Consumer<T> : IConsumer<Event<T>> {

            /// <summary>
            /// Create subscription
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="handler"></param>
            public Consumer(MassTransitEventBus outer, IEventHandler<T> handler) {
                _handler = handler;
                _outer = outer;
            }

            /// <inheritdoc/>
            public async Task Consume(ConsumeContext<Event<T>> context) {
                await _handler.HandleAsync(context.Message.Value);
                _outer._logger.Verbose("<-----  {@message} received and handled! ",
                    context.Message.Value);
            }

            private readonly IEventHandler<T> _handler;
            private readonly MassTransitEventBus _outer;
        }

        /// <summary>
        /// Event wrapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class Event<T> {
            public T Value { get; set; }
        }

        private readonly ConcurrentDictionary<string, ConnectHandle> _handlers =
            new ConcurrentDictionary<string, ConnectHandle>();
        private readonly ILogger _logger;
        private readonly IPublishEndpointProvider _publisher;
        private readonly IRegistration _registration;
        private readonly IConsumePipeConnector _connector;
        private readonly IReceiveEndpointConfigurator _config;
    }
}