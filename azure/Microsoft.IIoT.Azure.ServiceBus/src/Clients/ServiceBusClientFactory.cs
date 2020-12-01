// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Azure.ServiceBus.Clients {
    using Microsoft.IIoT.Utils;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Create service bus clients
    /// </summary>
    public sealed class ServiceBusClientFactory : IServiceBusClientFactory,
        IDisposable, IAsyncDisposable {

        /// <summary>
        /// Connection string
        /// </summary>
        public string ServiceBusConnString => _config.CurrentValue.ServiceBusConnString;

        /// <summary>
        /// Create factory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ServiceBusClientFactory(IOptionsMonitor<ServiceBusOptions> options, ILogger logger) {
            _config = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ISubscriptionClient> CreateOrGetSubscriptionClientAsync(
            Func<Message, CancellationToken, Task> handler,
            Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler,
            string name, string topic) {
            topic = GetEntityName(topic);
            if (string.IsNullOrEmpty(name)) {
                name = Dns.GetHostName();
            }
            await _subscriptionLock.WaitAsync().ConfigureAwait(false);
            try {
                var key = $"{topic}/subscriptions/{name}";
                if (!_subscriptionClients.TryGetValue(key, out var client) ||
                    client.IsClosedOrClosing) {
                    client = await NewSubscriptionClientAsync(topic, name).ConfigureAwait(false);
                    _subscriptionClients.AddOrUpdate(key, client);

                    //
                    // TODO: Should also check whether the handlers are different
                    // and close/create new subscription handler...
                    //
                    client.RegisterMessageHandler(handler,
                        new MessageHandlerOptions(exceptionReceivedHandler) {
                            MaxConcurrentCalls = 2,
                            MaxAutoRenewDuration = TimeSpan.FromMinutes(1),
                            AutoComplete = false
                        });
                }
                return client;
            }
            finally {
                _subscriptionLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<ITopicClient> CreateOrGetTopicClientAsync(string topic) {
            topic = GetEntityName(topic);
            await _topicLock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_topicClients.TryGetValue(topic, out var client) ||
                    client.IsClosedOrClosing) {
                    client = await NewTopicClientAsync(topic).ConfigureAwait(false);
                    _topicClients.AddOrUpdate(topic, client);
                }
                return client;
            }
            finally {
                _topicLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IQueueClient> CreateOrGetGetQueueClientAsync(string queue) {
            queue = GetEntityName(queue);
            await _queueLock.WaitAsync().ConfigureAwait(false);
            try {
                if (!_queueClients.TryGetValue(queue, out var client) ||
                    client.IsClosedOrClosing) {
                    client = await NewQueueClientAsync(queue).ConfigureAwait(false);
                    _queueClients.AddOrUpdate(queue, client);
                }
                return client;
            }
            finally {
                _queueLock.Release();
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            await Task.WhenAll(
                CloseAllAsync(_queueLock, _queueClients),
                CloseAllAsync(_topicLock, _topicClients),
                CloseAllAsync(_subscriptionLock, _subscriptionClients)).ConfigureAwait(false);

            _queueLock.Dispose();
            _topicLock.Dispose();
            _subscriptionLock.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose() {
            DisposeAsync().AsTask().Wait();
        }

        /// <summary>
        /// Create subscription client
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<ISubscriptionClient> NewSubscriptionClientAsync(
            string topic, string name) {
            var managementClient = new ManagementClient(ServiceBusConnString);
            while (true) {
                try {
                    var exists = await managementClient.TopicExistsAsync(topic).ConfigureAwait(false);
                    if (!exists) {
                        await managementClient.CreateTopicAsync(new TopicDescription(topic) {
                            EnablePartitioning = true,
                            EnableBatchedOperations = true
                        }).ConfigureAwait(false);
                    }
                    exists = await managementClient.SubscriptionExistsAsync(topic, name).ConfigureAwait(false);
                    if (!exists) {
                        await managementClient.CreateSubscriptionAsync(
                            new SubscriptionDescription(topic, name) {
                                EnableBatchedOperations = true,
                                LockDuration = TimeSpan.FromSeconds(10)
                            }).ConfigureAwait(false);
                    }
                    return new SubscriptionClient(ServiceBusConnString, topic, name,
                        ReceiveMode.PeekLock, RetryPolicy.Default);
                }
                catch (ServiceBusException ex) {
                    if (IsRetryableException(ex)) {
                        await Task.Delay(2000).ConfigureAwait(false);
                        continue;
                    }
                    _logger.LogError(ex, "Failed to create subscription client.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Create queue client
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<IQueueClient> NewQueueClientAsync(string name) {
            var managementClient = new ManagementClient(ServiceBusConnString);
            while (true) {
                try {
                    var exists = await managementClient.QueueExistsAsync(name).ConfigureAwait(false);
                    if (!exists) {
                        await managementClient.CreateQueueAsync(new QueueDescription(name) {
                            EnablePartitioning = true,
                            EnableBatchedOperations = true
                        }).ConfigureAwait(false);
                    }
                    return new QueueClient(
                        ServiceBusConnString, GetEntityName(name), ReceiveMode.PeekLock,
                            RetryPolicy.Default);
                }
                catch (ServiceBusException ex) {
                    if (IsRetryableException(ex)) {
                        await Task.Delay(2000).ConfigureAwait(false);
                        continue; // 429
                    }
                    _logger.LogError(ex, "Failed to create queue client.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Create topic client
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<TopicClient> NewTopicClientAsync(string name) {
            var managementClient = new ManagementClient(ServiceBusConnString);
            while (true) {
                try {
                    var exists = await managementClient.TopicExistsAsync(name).ConfigureAwait(false);
                    if (!exists) {
                        await managementClient.CreateTopicAsync(new TopicDescription(name) {
                            EnablePartitioning = true,
                            EnableBatchedOperations = true
                        }).ConfigureAwait(false);
                    }
                    return new TopicClient(ServiceBusConnString, GetEntityName(name),
                        RetryPolicy.Default);
                }
                catch (ServiceBusException ex) {
                    if (IsRetryableException(ex)) {
                        await Task.Delay(2000).ConfigureAwait(false);
                        continue; // 429
                    }
                    _logger.LogError(ex, "Failed to create topic client.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Check whether the subcode points to 429
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool IsRetryableException(ServiceBusException ex) {
            return
    // https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-resource-manager-exceptions
            ex.Message.StartsWith("Resource Conflict", StringComparison.InvariantCultureIgnoreCase) ||
                ex.Message.StartsWith("SubCode=40900", StringComparison.InvariantCultureIgnoreCase) ||
                ex.Message.StartsWith("SubCode=40901", StringComparison.InvariantCultureIgnoreCase) ||
                ex.Message.StartsWith("SubCode=50004", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Close all clients in list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientLock"></param>
        /// <param name="clients"></param>
        /// <returns></returns>
        private static async Task CloseAllAsync<T>(SemaphoreSlim clientLock,
            Dictionary<string, T> clients) where T : IClientEntity {
            await clientLock.WaitAsync().ConfigureAwait(false);
            try {
                foreach (var client in clients.Values) {
                    await Try.Async(() => client.CloseAsync()).ConfigureAwait(false);
                }
                clients.Clear();
            }
            finally {
                clientLock.Release();
            }
        }

        /// <summary>
        /// Get entity path
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetEntityName(string name) {
            if (string.IsNullOrEmpty(name)) {
                var cs = new ServiceBusConnectionStringBuilder(ServiceBusConnString);
                name = cs.EntityPath;
            }
            if (string.IsNullOrEmpty(name)) {
                name = "iiotmessaging";
            }
            return name.ToUpperInvariant();
        }

        private readonly IOptionsMonitor<ServiceBusOptions> _config;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _topicLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, ITopicClient> _topicClients =
            new Dictionary<string, ITopicClient>();
        private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, IQueueClient> _queueClients =
            new Dictionary<string, IQueueClient>();
        private readonly SemaphoreSlim _subscriptionLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, ISubscriptionClient> _subscriptionClients =
            new Dictionary<string, ISubscriptionClient>();
    }
}