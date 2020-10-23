// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.ServiceBus.Services {
    using Microsoft.Azure.IIoT.Messaging;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Utils;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event bus built on top of service bus
    /// </summary>
    public sealed class ServiceBusEventBus : IEventBus, IDisposable {

        /// <summary>
        /// Create service bus event bus
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="process"></param>
        public ServiceBusEventBus(IServiceBusClientFactory factory, IJsonSerializer serializer,
            ILogger logger, IProcessIdentity process, IServiceBusEventBusConfig config = null) {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _config = config;

            if (string.IsNullOrEmpty(process?.ServiceId)) {
                throw new ArgumentNullException(nameof(process));
            }

            // Create subscription client
            _subscriptionClient = _factory.CreateOrGetSubscriptionClientAsync(
                ProcessEventAsync, ExceptionReceivedHandler, process.ServiceId,
                    _config?.Topic).Result;
            Try.Async(() => _subscriptionClient.RemoveRuleAsync(
                RuleDescription.DefaultRuleName)).Wait();
        }

        /// <inheritdoc/>
        public async Task PublishAsync<T>(T message) {
            if (message is null) {
                throw new ArgumentNullException(nameof(message));
            }
            var body = _serializer.SerializeToBytes(message).ToArray();
            try {
                var client = await _factory.CreateOrGetTopicClientAsync(_config?.Topic).ConfigureAwait(false);

                var moniker = typeof(T).GetMoniker();
                await client.SendAsync(new Message {
                    MessageId = Guid.NewGuid().ToString(),
                    Body = body,
                    PartitionKey = moniker,
                    Label = moniker,
                }).ConfigureAwait(false);

                _logger.Verbose("----->  {@message} sent...", message);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Failed to publish message {@message}", message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CloseAsync() {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                foreach (var handlers in _handlers) {
                    var eventName = handlers.Key;
                    try {
                        await _subscriptionClient.RemoveRuleAsync(eventName).ConfigureAwait(false);
                    }
                    catch (MessagingEntityNotFoundException) {
                        _logger.Warning("The messaging entity {eventName} could not be found.",
                            eventName);
                    }
                }
                _handlers.Clear();
                if (_subscriptionClient.IsClosedOrClosing) {
                    return;
                }
                await _subscriptionClient.CloseAsync().ConfigureAwait(false);
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            Try.Async(CloseAsync).Wait();
            _lock.Dispose();
        }

        /// <inheritdoc/>
        public async Task<string> RegisterAsync<T>(IEventHandler<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            var eventName = typeof(T).GetMoniker();
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_handlers.TryGetValue(eventName, out var handlers)) {
                    try {
                        await _subscriptionClient.AddRuleAsync(new RuleDescription {
                            Filter = new CorrelationFilter { Label = eventName },
                            Name = eventName
                        }).ConfigureAwait(false);
                    }
                    catch (ServiceBusException ex) {
                        if (ex.Message.Contains("already exists")) {
                            _logger.Debug("The messaging entity {eventName} already exists.",
                                eventName);
                        }
                        else {
                            throw;
                        }
                    }
                    handlers = new Dictionary<string, Subscription>();
                    _handlers.Add(eventName, handlers);
                }
                var token = Guid.NewGuid().ToString();
                handlers.Add(token, new Subscription {
                    HandleAsync = e => handler.HandleAsync((T)e),
                    Type = typeof(T)
                });
                return token;
            }
            finally {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task UnregisterAsync(string token) {
            if (string.IsNullOrEmpty(token)) {
                throw new ArgumentNullException(nameof(token));
            }
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                string eventName = null;
                var found = false;
                foreach (var subscriptions in _handlers) {
                    eventName = subscriptions.Key;
                    if (subscriptions.Value.TryGetValue(token, out var subscription)) {
                        found = true;
                        // Remove handler
                        subscriptions.Value.Remove(token);
                        if (subscriptions.Value.Count != 0) {
                            eventName = null; // Do not clean up yet
                        }
                        break;
                    }
                }
                if (!found) {
                    throw new ResourceInvalidStateException("Token not found");
                }
                if (string.IsNullOrEmpty(eventName)) {
                    return; // No more action
                }
                try {
                    await _subscriptionClient.RemoveRuleAsync(eventName).ConfigureAwait(false);
                }
                catch (ServiceBusException) {
                    _logger.Warning("The messaging entity {eventName} does not exist.",
                        eventName);
                    // TODO: throw?
                }
                _handlers.Remove(eventName);
            }
            finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Process exceptions
        /// </summary>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs eventArg) {
            var ex = eventArg.Exception;
            var context = eventArg.ExceptionReceivedContext;
            _logger.Error(ex, "{ExceptionMessage} - Context: {@ExceptionContext}",
                ex.Message, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Process event
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProcessEventAsync(Message message, CancellationToken token) {
            IEnumerable<Subscription> subscriptions = null;
            await _lock.WaitAsync(token).ConfigureAwait(false);
            try {
                if (!_handlers.TryGetValue(message.Label, out var handlers)) {
                    return;
                }
                subscriptions = handlers.Values.ToList();
            }
            finally {
                _lock.Release();
            }
            foreach (var handler in subscriptions) {
                // Do for now every time to pass brand new objects
                var evt = _serializer.Deserialize(message.Body, handler.Type);
                await handler.HandleAsync(evt).ConfigureAwait(false);
                _logger.Verbose("<-----  {@message} received and handled! ", evt);
            }
            // Complete the message so that it is not received again.
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscription holder
        /// </summary>
        private class Subscription {

            /// <summary>
            /// Event type
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// Untyped handler
            /// </summary>
            public Func<object, Task> HandleAsync { get; set; }
        }

        private readonly Dictionary<string, Dictionary<string, Subscription>> _handlers =
            new Dictionary<string, Dictionary<string, Subscription>>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly IServiceBusClientFactory _factory;
        private readonly IServiceBusEventBusConfig _config;
        private readonly ISubscriptionClient _subscriptionClient;
    }
}