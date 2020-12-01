// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Orleans.Grains {
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System;
    using global::Orleans;
    using global::Orleans.Concurrency;

    /// <summary>
    /// Topic implementation
    /// </summary>
    [Serializable]
    [Reentrant]
    public class OrleansTopicGrain : Grain, IOrleansTopic {

        /// <summary>
        /// Create topic
        /// </summary>
        /// <param name="logger"></param>
        public OrleansTopicGrain(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptions = new ConcurrentDictionary<IOrleansSubscription, bool>();
        }

        /// <inheritdoc/>
        public Task PublishAsync(byte[] buffer) {
            foreach (var subscription in _subscriptions.Keys) {
                try {
                    subscription.Consume(buffer);
                    _logger.LogTrace("Sent message through topic to {observer}", 
                        subscription.GetPrimaryKey());
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Failed to push message to subscription");
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SubscribeAsync(IOrleansSubscription subscription) {
            _subscriptions.TryAdd(subscription, true);
            _logger.LogInformation("Subscribed {observer} to topic",
                subscription.GetPrimaryKey());
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UnsubscribeAsync(IOrleansSubscription subscription) {
            _subscriptions.TryRemove(subscription, out _);
            _logger.LogInformation("Unsubscribed {observer} from topic", 
                subscription.GetPrimaryKey());
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task OnActivateAsync() {
            return base.OnActivateAsync();
        }

        /// <inheritdoc/>
        public override Task OnDeactivateAsync() {
            return base.OnDeactivateAsync();
        }

        private readonly ConcurrentDictionary<IOrleansSubscription, bool> _subscriptions;
        private readonly ILogger _logger;
    }
}