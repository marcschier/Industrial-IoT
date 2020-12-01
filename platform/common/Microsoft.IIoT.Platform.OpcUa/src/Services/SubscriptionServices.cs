// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa.Services {
    using Microsoft.IIoT.Platform.OpcUa.Models;
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Utils;
    using Opc.Ua;
    using Opc.Ua.Client;
    using Opc.Ua.Extensions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Subscription services implementation
    /// </summary>
    public sealed class SubscriptionServices : ISubscriptionClient, IDisposable {

        /// <inheritdoc/>
        public int TotalSubscriptionCount => _subscriptions.Count;

        /// <summary>
        /// Create subscription manager
        /// </summary>
        /// <param name="sessionManager"></param>
        /// <param name="codec"></param>
        /// <param name="logger"></param>
        public SubscriptionServices(ISessionManager sessionManager, IVariantEncoderFactory codec,
            ILogger logger) {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task<ISubscriptionHandle> CreateSubscriptionAsync(SubscriptionModel subscriptionModel, ISubscriptionListener listener) {
            if (subscriptionModel is null) {
                throw new ArgumentNullException(nameof(subscriptionModel));
            }
            if (string.IsNullOrEmpty(subscriptionModel.Id)) {
                throw new ArgumentException("Missing id field", nameof(subscriptionModel));
            }
            var sub = _subscriptions.GetOrAdd(subscriptionModel.Id,
                key => new SubscriptionWrapper(this, listener, subscriptionModel, _logger));
            _sessionManager.RegisterSubscription(sub);
            return Task.FromResult<ISubscriptionHandle>(sub);
        }

        /// <inheritdoc/>
        public void Dispose() {
            // Cleanup remaining subscriptions
            var subscriptions = _subscriptions.Values.ToList();
            _subscriptions.Clear();
            subscriptions.ForEach(s => Try.Op(() => _sessionManager.UnregisterSubscription(s)));
            subscriptions.ForEach(s => Try.Op(() => s.Dispose()));
        }

        /// <summary>
        /// Subscription implementation
        /// </summary>
        internal sealed class SubscriptionWrapper : ISubscriptionHandle {

            /// <inheritdoc/>
            public string Id => _subscription.Id;

            /// <inheritdoc/>
            public bool Enabled { get; private set; }

            /// <inheritdoc/>
            public bool Active { get; private set; }

            /// <inheritdoc/>
            public ConnectionModel Connection => _subscription.Connection;

            /// <summary>
            /// Subscription wrapper
            /// </summary>
            /// <param name="outer"></param>
            /// <param name="listener"></param>
            /// <param name="subscription"></param>
            /// <param name="logger"></param>
            public SubscriptionWrapper(SubscriptionServices outer, ISubscriptionListener listener,
                SubscriptionModel subscription, ILogger logger) {
                _subscription = subscription.Clone() ??
                    throw new ArgumentNullException(nameof(subscription));
                _outer = outer ??
                    throw new ArgumentNullException(nameof(outer));
                _listener = listener ??
                    throw new ArgumentNullException(nameof(listener));
                _logger = logger ??
                    throw new ArgumentNullException(nameof(logger));
                _lock = new SemaphoreSlim(1, 1);
            }

            /// <inheritdoc/>
            public void UpdateConnectivityState(ConnectionStatus previous,
                ConnectionStatus newValue) {
                _logger.LogInformation("Subscription {id} connectivitiy state changed from" +
                    " {previous} to {newValue}", _subscription.Id, previous, newValue);
                _listener.OnConnectivityChange(previous, newValue);
            }

            /// <inheritdoc/>
            public async Task CloseAsync() {
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    _logger.LogInformation("Closing subscription {subscription}", Id);
                    _outer._sessionManager.UnregisterSubscription(this);
                    _outer._subscriptions.TryRemove(Id, out _);
                }
                finally {
                    _lock.Release();
                }
                try {
                    var session = _outer._sessionManager.GetOrCreateSession(_subscription.Connection, false);
                    if (session != null) {
                        var subscription = session.Subscriptions.
                            SingleOrDefault(s => s.Handle == this);
                        if (subscription != null) {
                            Try.Op(() => subscription.PublishingEnabled = false);
                            Try.Op(() => subscription.ApplyChanges());
                            Try.Op(() => subscription.DeleteItems());
                            _logger.LogDebug("Deleted monitored items for {subscription}", Id);
                            Try.Op(() => session?.RemoveSubscription(subscription));
                            _logger.LogDebug("Subscription successfully removed {subscription}", Id);
                        }
                    }
                }
                catch (Exception e) {
                    _logger.LogError(e, "Failed to close subscription {subscription}", Id);
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                Try.Async(CloseAsync).Wait();
                _lock.Dispose();
            }

            /// <inheritdoc/>
            public async Task ApplyAsync(IEnumerable<MonitoredItemModel> monitoredItems,
                SubscriptionConfigurationModel configuration) {
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    // set the new set of monitored items
                    _subscription.MonitoredItems = monitoredItems?.Select(n => n.Clone()).ToList();

                    // try to get the subscription with the new configuration
                    var session = _outer._sessionManager.GetOrCreateSession(_subscription.Connection, true);
                    var rawSubscription = GetSubscription(session, configuration, Active);
                    if (session == null || rawSubscription == null) {
                        Enabled = false;
                        Active = false;
                    }
                    else {
                        ResolveDisplayNames(session);
                        Active = await SetMonitoredItemsAsync(rawSubscription, _subscription.MonitoredItems, Active)
                            .ConfigureAwait(false) && Active;

                        _listener.OnSubscriptionStatusChange(_subscription.Id, rawSubscription, null);
                    }
                }
                catch (ServiceResultException sre) {
                    _logger.LogError("Failed to apply monitored items due to {exception}", sre.Message);
                    Enabled = false;
                    Active = false;
                    NotifySubscriptionError(sre);
                }
                catch (Exception e) {
                    _logger.LogError("Failed to apply monitored items due to {exception}", e.Message);
                    Enabled = false;
                    Active = false;
                }
                finally {
                    _lock.Release();
                    // just give the control to the session manager
                    _outer._sessionManager.RegisterSubscription(this);
                }
            }

            /// <inheritdoc/>
            public async Task EnableAsync(Session session) {
                try {
                    ResolveDisplayNames(session);
                    Active = await ReapplyAsync(session, false).ConfigureAwait(false);
                    Enabled = true;
                }
                catch (Exception e) {
                    _logger.LogError(e, "Failed to enable subscription");
                    Enabled = false;
                    Active = false;
                }
            }

            /// <inheritdoc/>
            public async Task ActivateAsync(Session session) {
                try {
                    if (!Enabled) {
                        // force a reactivation
                        Active = false;
                    }
                    if (!Active) {
                        Active = await ReapplyAsync(session, true).ConfigureAwait(false);
                        Enabled = true;
                    }
                }
                catch (Exception e) {
                    _logger.LogError(e, "Failed to activate subscription");
                    Enabled = false;
                    Active = false;
                }
            }

            /// <inheritdoc/>
            public async Task DeactivateAsync(Session session) {
                try {
                    Active = await ReapplyAsync(session, false).ConfigureAwait(false);
                }
                catch (Exception e) {
                    _logger.LogError(e, "Failed to deactivate subscription");
                    Enabled = false;
                    Active = false;
                }
            }

            /// <summary>
            /// sanity check of the subscription
            /// </summary>
            /// <returns></returns>
            private async Task<bool> ReapplyAsync(Session session, bool activate) {
                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    var rawSubscription = GetSubscription(session, null, activate);
                    if (rawSubscription == null) {
                        return false;
                    }
                    var result = await SetMonitoredItemsAsync(rawSubscription, _subscription.MonitoredItems, activate)
                        .ConfigureAwait(false) && activate;
                    _listener.OnSubscriptionStatusChange(_subscription.Id, rawSubscription);
                    return result;
                }
                catch (ServiceResultException sre) {
                    _logger.LogError("Failed to reapply monitored items due to {exception}", sre.Message);
                    NotifySubscriptionError(sre);
                    throw;
                }
                finally {
                    _lock.Release();
                }
            }

            /// <summary>
            /// reads the display name of the nodes to be monitored
            /// </summary>
            /// <param name="session"></param>
            /// <returns></returns>
            private void ResolveDisplayNames(Session session) {
                if (!(_subscription?.Configuration?.ResolveDisplayName ?? false)) {
                    return;
                }

                if (session == null) {
                    return;
                }

                var unresolvedMonitoredItems = _subscription.MonitoredItems
                    .Where(mi => string.IsNullOrEmpty(mi.DisplayName));
                if (!unresolvedMonitoredItems.Any()) {
                    return;
                }

                try {
                    var nodeIds = unresolvedMonitoredItems.
                        Select(n => {
                            try {
                                return n.StartNodeId.ToNodeId(session.MessageContext);
                            }
                            catch (ServiceResultException sre) {
                                _logger.LogWarning("Failed to resolve display name for '{monitoredItem}' due to '{message}'",
                                    n.StartNodeId, sre.Message);
                            }
                            catch (Exception e) {
                                _logger.LogError(e, "Failed to resolve display name for '{monitoredItem}'",
                                    n.StartNodeId);
                                throw;
                            }
                            return null;
                        });
                    if (nodeIds.Any()) {
                        session.ReadDisplayName(nodeIds.ToList(), out var displayNames, out var errors);
                        var index = 0;
                        foreach (var monitoredItem in unresolvedMonitoredItems) {
                            if (StatusCode.IsGood(errors[index].StatusCode)) {
                                monitoredItem.DisplayName = displayNames[index];
                            }
                            else {
                                monitoredItem.DisplayName = null;
                                _logger.LogWarning("Failed to read display name for '{monitoredItem}' due to '{statusCode}'",
                                    monitoredItem.StartNodeId, errors[index]);
                            }
                            index++;
                        }
                    }
                }
                catch (ServiceResultException sre) {
                    _logger.LogWarning("Failed to resolve display names for monitored items due to '{message}'",
                        sre.Message);
                }
                catch (Exception e) {
                    _logger.LogError(e, "Failed to resolve display names for monitored items");
                    throw;
                }
            }

            /// <summary>
            /// Synchronize monitored items and triggering configuration in subscription
            /// </summary>
            /// <param name="rawSubscription"></param>
            /// <param name="monitoredItems"></param>
            /// <param name="activate"></param>
            /// <returns></returns>
            private async Task<bool> SetMonitoredItemsAsync(Subscription rawSubscription,
                IEnumerable<MonitoredItemModel> monitoredItems, bool activate) {

                var currentState = rawSubscription.MonitoredItems
                    .Select(m => m.Handle)
                    .OfType<MonitoredItemWrapper>()
                    .ToHashSetSafe();

                var applyChanges = false;
                var noErrorFound = true;
                var codec = _outer._codec.Create(rawSubscription.Session.MessageContext);
                if (monitoredItems == null || !monitoredItems.Any()) {
                    // cleanup
                    var toCleanupList = currentState.Select(t => t.Item).ToList();
                    if (toCleanupList.Count != 0) {
                        // Remove monitored items not in desired state
                        rawSubscription.RemoveItems(toCleanupList);
                        _logger.LogInformation("Removed {count} monitored items in subscription "
                            + "{subscription}", toCleanupList.Count, rawSubscription.DisplayName);
                    }
                    _currentlyMonitored = null;
                    rawSubscription.ApplyChanges();
                    rawSubscription.SetPublishingMode(false);
                    if (rawSubscription.MonitoredItemCount != 0) {
                        _logger.LogWarning("Failed to remove {count} monitored items from subscription "
                            + "{subscription}", rawSubscription.MonitoredItemCount, rawSubscription.DisplayName);
                    }
                    return noErrorFound;
                }

                // Synchronize the desired items with the state of the raw subscription
                var desiredState = monitoredItems
                    .Select(m => new MonitoredItemWrapper(m, _logger))
                    .ToHashSetSafe();

                var toRemoveList = currentState.Except(desiredState).Select(t => t.Item).ToList();
                if (toRemoveList.Count != 0) {
                    rawSubscription.RemoveItems(toRemoveList);
                    applyChanges = true;
                    _logger.LogInformation("Removed {count} monitored items from subscription "
                        + "{subscription}", toRemoveList.Count, rawSubscription.DisplayName);
                }

                // todo re-associate detached handles!?
                var toRemoveDetached = rawSubscription.MonitoredItems.Where(m => m.Status == null).ToList();
                if (toRemoveDetached.Count != 0) {
                    _logger.LogInformation("Removed {count} detached monitored items from subscription "
                        + "{subscription}", toRemoveDetached.Count, rawSubscription.DisplayName);
                    rawSubscription.RemoveItems(toRemoveDetached);
                }

                var nowMonitored = new List<MonitoredItemWrapper>();
                var toAddList = desiredState.Except(currentState).ToList();
                if (toAddList.Count != 0) {
                    // Add new monitored items not in current state
                    foreach (var toAdd in toAddList) {
                        // Create monitored item
                        if (!activate) {
                            toAdd.Template.MonitoringMode = Publisher.Models.MonitoringMode.Disabled;
                        }
                        try {
                            toAdd.Create(rawSubscription.Session, codec, activate);
                            nowMonitored.Add(toAdd);
                            _logger.LogTrace("Adding new monitored item '{item}'...",
                                toAdd.Item.StartNodeId);
                        }
                        catch (ServiceResultException sre) {
                            _logger.LogWarning("Failed to add new monitored item '{item}' due to '{message}'",
                                toAdd.Template.StartNodeId, sre.Message);
                        }
                        catch (Exception e) {
                            _logger.LogError(e, "Failed to add new monitored item '{item}'",
                                toAdd.Template.StartNodeId);
                            throw;
                        }
                    }
                    rawSubscription.AddItems(
                        toAddList.Where(t => t?.Item != null).Select(t => t.Item).ToList());
                    applyChanges = true;
                    _logger.LogInformation("Added {count} monitored items to subscription "
                        + "{subscription}", toAddList.Count, rawSubscription.DisplayName);
                }

                // Update monitored items that have changed
                var desiredUpdates = desiredState.Intersect(currentState)
                    .ToDictionary(k => k, v => v);
                var count = 0;
                foreach (var toUpdate in currentState.Intersect(desiredState)) {
                    if (toUpdate.MergeWith(desiredUpdates[toUpdate])) {
                        _logger.LogTrace("Updating monitored item '{item}'...", toUpdate);
                        count++;
                    }
                    nowMonitored.Add(toUpdate);
                }
                if (count > 0) {
                    applyChanges = true;
                    _logger.LogInformation("Updated {count} monitored items in subscription "
                        + "{subscription}", count, rawSubscription.DisplayName);
                }

                if (applyChanges) {
                    rawSubscription.ApplyChanges();

                    foreach (var item in currentState.Concat(nowMonitored).Distinct()) {
                        item.UpdateStatus(_listener, _subscription.Id, rawSubscription);
                    }

                    _currentlyMonitored = nowMonitored;
                    if (!activate) {
                        var map = _currentlyMonitored.ToDictionary(
                            k => k.Template.Id ?? k.Template.StartNodeId, v => v);
                        foreach (var item in _currentlyMonitored.ToList()) {
                            if (item.Template.TriggerId != null &&
                                map.TryGetValue(item.Template.TriggerId, out var trigger)) {
                                trigger?.AddTriggerLink(item.ServerId.GetValueOrDefault());
                            }
                        }

                        // Set up any new trigger configuration if needed
                        foreach (var item in _currentlyMonitored.ToList()) {
                            if (item.GetTriggeringLinks(out var added, out var removed)) {
                                var response = await rawSubscription.Session.SetTriggeringAsync(
                                    null, rawSubscription.Id, item.ServerId.GetValueOrDefault(),
                                    new UInt32Collection(added),
                                    new UInt32Collection(removed)).ConfigureAwait(false);
                            }
                        }

                        // sanity check
                        foreach (var monitoredItem in _currentlyMonitored) {
                            if (monitoredItem.Item.Status.Error != null &&
                                StatusCode.IsNotGood(monitoredItem.Item.Status.Error.StatusCode)) {
                                _logger.LogWarning("Error monitoring node {id} due to {code} in subscription " +
                                    "{subscription}", monitoredItem.Item.StartNodeId,
                                    monitoredItem.Item.Status.Error.StatusCode, rawSubscription.DisplayName);
                                monitoredItem.Template.MonitoringMode = Publisher.Models.MonitoringMode.Disabled;
                                noErrorFound = false;
                            }
                        }

                        foreach (var item in _currentlyMonitored) {
                            item.UpdateStatus(_listener, _subscription.Id, rawSubscription);
                        }

                        count = _currentlyMonitored.Count(m => m.Item.Status.Error == null);

                        _logger.LogInformation("Now monitoring {count} nodes in subscription " +
                            "{subscription}", count, rawSubscription.DisplayName);

                        if (_currentlyMonitored.Count != rawSubscription.MonitoredItemCount) {
                            _logger.LogError("Monitored items mismatch: wrappers{wrappers} != items:{items} ",
                                _currentlyMonitored.Count, _currentlyMonitored.Count);
                        }
                    }
                }
                else {
                    // do a sanity check
                    foreach (var monitoredItem in _currentlyMonitored) {
                        if (monitoredItem.Item.Status.Error != null &&
                            StatusCode.IsNotGood(monitoredItem.Item.Status.Error.StatusCode)) {
                            monitoredItem.Template.MonitoringMode = Publisher.Models.MonitoringMode.Disabled;
                            noErrorFound = false;
                            applyChanges = true;
                        }
                    }
                    if (applyChanges) {
                        rawSubscription.ApplyChanges();

                        foreach (var item in _currentlyMonitored) {
                            item.UpdateStatus(_listener, _subscription.Id, rawSubscription);
                        }
                    }
                }

                if (activate) {
                    // Change monitoring mode of all valid items if necessary
                    var validItems = _currentlyMonitored.Where(v => v.Item.Status.Error == null);
                    foreach (var change in validItems.GroupBy(i => i.GetMonitoringModeChange())) {
                        if (change.Key == null) {
                            continue;
                        }
                        var nodes = change.Select(t => t.Item).ToList();
                        _logger.LogInformation("Set Monitoring to {value} for {count} nodes in subscription " +
                            "{subscription}", change.Key.Value, nodes.Count, rawSubscription.DisplayName);
                        var results = rawSubscription.SetMonitoringMode(change.Key.Value, nodes);
                        if (results != null) {
                            _logger.LogDebug("Failed to set monitoring for {count} nodes in subscription {subscription}",
                                results.Count(r => r != null && StatusCode.IsNotGood(r.StatusCode)),
                                rawSubscription.DisplayName);
                        }

                        foreach (var item in change) {
                            item.UpdateStatus(_listener, _subscription.Id, rawSubscription);
                        }
                    }
                }
                return noErrorFound;
            }

            private static uint GreatCommonDivisor(uint a, uint b) {
                return b == 0 ? a : GreatCommonDivisor(b, a % b);
            }

            /// <summary>
            /// Retrieve a raw subscription with all settings applied (no lock)
            /// </summary>
            /// <param name="session"></param>
            /// <param name="configuration"></param>
            /// <param name="activate"></param>
            /// <returns></returns>
            private Subscription GetSubscription(Session session,
                SubscriptionConfigurationModel configuration, bool activate) {

                if (configuration != null) {
                    // Apply new configuration right here saving us from modifying later
                    _subscription.Configuration = configuration.Clone();
                }

                if (session == null) {
                    session = _outer._sessionManager.GetOrCreateSession(_subscription.Connection, true);
                    if (session == null) {
                        return null;
                    }
                }

                // calculate the KeepAliveCount no matter what, perhaps monitored items were changed
                var revisedKeepAliveCount = _subscription.Configuration.KeepAliveCount
                    .GetValueOrDefault(session.DefaultSubscription.KeepAliveCount);
                _subscription.MonitoredItems?.ForEach(m => {
                    if (m.HeartbeatInterval != null && m.HeartbeatInterval != TimeSpan.Zero) {
                        var itemKeepAliveCount = (uint)m.HeartbeatInterval.Value.TotalMilliseconds /
                            (uint)_subscription.Configuration.PublishingInterval.Value.TotalMilliseconds;
                        revisedKeepAliveCount = GreatCommonDivisor(revisedKeepAliveCount, itemKeepAliveCount);
                    }
                });

                var subscription = session.Subscriptions.SingleOrDefault(s => s.Handle == this);
                if (subscription == null) {
                    subscription = new Subscription(session.DefaultSubscription) {
                        Handle = this,
                        DisplayName = Id,
                        PublishingEnabled = activate, // false on initialization
                        KeepAliveCount = revisedKeepAliveCount,
                        FastDataChangeCallback = OnSubscriptionDataChanged,
                        FastEventCallback = OnSubscriptionEventChanged,
                        DisableMonitoredItemCache = true,
                        PublishingInterval = (int)_subscription.Configuration.PublishingInterval
                            .GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalMilliseconds,
                        MaxNotificationsPerPublish = _subscription.Configuration.MaxNotificationsPerPublish
                            .GetValueOrDefault(0),
                        LifetimeCount = _subscription.Configuration.LifetimeCount
                            .GetValueOrDefault(session.DefaultSubscription.LifetimeCount),
                        Priority = _subscription.Configuration.Priority
                            .GetValueOrDefault(session.DefaultSubscription.Priority)
                    };
                    var result = session.AddSubscription(subscription);
                    if (!result) {
                        _logger.LogError("Failed to add subscription '{name}' to session '{session}'",
                             Id, session.SessionName);
                        subscription = null;
                    }
                    else {
                        subscription.Create();
                        //TODO - add logs for the revised values
                        _logger.LogDebug("Added subscription '{name}' to session '{session}'",
                             Id, session.SessionName);
                    }
                }
                else {
                    // Apply new configuration on configuration on original subscription
                    var modifySubscription = false;

                    if (revisedKeepAliveCount != subscription.KeepAliveCount) {
                        _logger.LogDebug("{subscription} Changing KeepAlive Count from {old} to {new}",
                            _subscription.Id, _subscription.Configuration?.KeepAliveCount ?? 0,
                            revisedKeepAliveCount);

                        subscription.KeepAliveCount = revisedKeepAliveCount;
                        modifySubscription = true;
                    }
                    if (subscription.PublishingInterval != (int)_subscription.Configuration.PublishingInterval
                            .GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalMilliseconds) {
                        _logger.LogDebug("{subscription} Changing publishing interval from {old} to {new}",
                            _subscription.Id,
                            configuration?.PublishingInterval ?? TimeSpan.Zero);
                        subscription.PublishingInterval = (int)_subscription.Configuration.PublishingInterval
                            .GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalMilliseconds;

                        modifySubscription = true;
                    }
                    if (subscription.MaxNotificationsPerPublish !=
                            _subscription.Configuration.MaxNotificationsPerPublish.GetValueOrDefault(0)) {
                        _logger.LogDebug("{subscription} Changing Max NotificationsPerPublish from {old} to {new}",
                            _subscription.Id, _subscription.Configuration?.MaxNotificationsPerPublish ?? 0,
                            configuration?.MaxNotificationsPerPublish ?? 0);
                        subscription.MaxNotificationsPerPublish =
                            _subscription.Configuration.MaxNotificationsPerPublish.GetValueOrDefault(0);
                        modifySubscription = true;
                    }
                    if (subscription.LifetimeCount != _subscription.Configuration.LifetimeCount
                            .GetValueOrDefault(session.DefaultSubscription.LifetimeCount)) {
                        _logger.LogDebug("{subscription} Changing Lifetime Count from {old} to {new}",
                            _subscription.Id, _subscription.Configuration?.LifetimeCount ?? 0,
                            configuration?.LifetimeCount ?? 0);
                        subscription.LifetimeCount = _subscription.Configuration.LifetimeCount
                            .GetValueOrDefault(session.DefaultSubscription.LifetimeCount);
                        modifySubscription = true;
                    }
                    if (subscription.Priority != _subscription.Configuration.Priority
                            .GetValueOrDefault(session.DefaultSubscription.Priority)) {
                        _logger.LogDebug("{subscription} Changing Priority from {old} to {new}",
                            _subscription.Id, _subscription.Configuration?.Priority ?? 0,
                            configuration?.Priority ?? 0);
                        subscription.Priority = _subscription.Configuration.Priority
                            .GetValueOrDefault(session.DefaultSubscription.Priority);
                        modifySubscription = true;
                    }
                    if (modifySubscription) {
                        subscription.Modify();
                        //Todo - add logs for the revised values
                    }
                    if (subscription.CurrentPublishingEnabled != activate) {
                        // do not deactivate an already activated subscription
                        subscription.SetPublishingMode(activate);
                    }
                }
                return subscription;
            }

            /// <summary>
            /// Notify about subscription error
            /// </summary>
            /// <param name="sre"></param>
            /// <returns></returns>
            private void NotifySubscriptionError(ServiceResultException sre) {
                _listener.OnSubscriptionStatusChange(_subscription.Id, null,
                    sre.Result);
            }

            /// <summary>
            /// Subscription event changes
            /// </summary>
            /// <param name="subscription"></param>
            /// <param name="notification"></param>
            /// <param name="stringTable"></param>
            private void OnSubscriptionEventChanged(Subscription subscription,
                EventNotificationList notification, IList<string> stringTable) {
                if (_currentlyMonitored == null || notification == null) {
                    return;
                }
                _listener.OnSubscriptionNotification(_subscription.Id, subscription, notification,
                    stringTable);
            }

            /// <summary>
            /// Subscription data changed
            /// </summary>
            /// <param name="subscription"></param>
            /// <param name="notification"></param>
            /// <param name="stringTable"></param>
            private void OnSubscriptionDataChanged(Subscription subscription,
                DataChangeNotification notification, IList<string> stringTable) {
                if (_currentlyMonitored == null || notification == null) {
                    return;
                }
                _listener.OnSubscriptionNotification(_subscription.Id, subscription, notification,
                    stringTable);
            }

            private readonly SubscriptionModel _subscription;
            private readonly SubscriptionServices _outer;
            private readonly ISubscriptionListener _listener;
            private readonly ILogger _logger;
            private readonly SemaphoreSlim _lock;
            private List<MonitoredItemWrapper> _currentlyMonitored;
        }

        /// <summary>
        /// Monitored item
        /// </summary>
        private class MonitoredItemWrapper {

            /// <summary>
            /// Assigned monitored item id on server
            /// </summary>
            public uint? ServerId => Item?.Status.Id;

            /// <summary>
            /// Monitored item
            /// </summary>
            public MonitoredItemModel Template { get; }

            /// <summary>
            /// Monitored item created from template
            /// </summary>
            public MonitoredItem Item { get; private set; }

            /// <summary>
            /// Last published time
            /// </summary>
            public DateTime NextHeartbeat { get; private set; }

            /// <summary>
            /// validates if a heartbeat is required.
            /// A heartbeat will be forced for the very first time
            /// </summary>
            /// <returns></returns>
            public bool ValidateHeartbeat(DateTime currentPublish) {
                if (NextHeartbeat == DateTime.MaxValue) {
                    return false;
                }
                if (NextHeartbeat > currentPublish + TimeSpan.FromMilliseconds(50)) {
                    return false;
                }
                NextHeartbeat = TimeSpan.Zero < Template.HeartbeatInterval.GetValueOrDefault(TimeSpan.Zero) ?
                    currentPublish + Template.HeartbeatInterval.Value : DateTime.MaxValue;
                return true;
            }

            /// <summary>
            /// Create wrapper
            /// </summary>
            /// <param name="template"></param>
            /// <param name="logger"></param>
            public MonitoredItemWrapper(MonitoredItemModel template, ILogger logger) {
                _logger = logger ??
                    throw new ArgumentNullException(nameof(logger));
                Template = template.Clone() ??
                    throw new ArgumentNullException(nameof(template));
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) {
                if (obj is not MonitoredItemWrapper item) {
                    return false;
                }
                if (Template.Id != item.Template.Id) {
                    return false;
                }
                if (!Template.RelativePath.SequenceEqualsSafe(item.Template.RelativePath)) {
                    return false;
                }
                if (Template.StartNodeId != item.Template.StartNodeId) {
                    return false;
                }
                if (Template.IndexRange != item.Template.IndexRange) {
                    return false;
                }
                if (Template.AttributeId != item.Template.AttributeId) {
                    return false;
                }
                return true;
            }

            /// <inheritdoc/>
            public override int GetHashCode() {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string>.Default.GetHashCode(Template.Id);
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string[]>.Default.GetHashCode(Template.RelativePath?.ToArray());
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string>.Default.GetHashCode(Template.StartNodeId);
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<string>.Default.GetHashCode(Template.IndexRange);
                hashCode = (hashCode * -1521134295) +
                    EqualityComparer<NodeAttribute?>.Default.GetHashCode(Template.AttributeId);
                return hashCode;
            }

            /// <inheritdoc/>
            public override string ToString() {
                return $"Item {Template.Id ?? "<unknown>"}{ServerId}: '{Template.StartNodeId}'" +
                    $" - {(Item?.Status?.Created == true ? "" : "not ")}created";
            }

            /// <summary>
            /// Create new stack monitored item
            /// </summary>
            /// <param name="session"></param>
            /// <param name="codec"></param>
            /// <param name="activate"></param>
            /// <returns></returns>
            internal void Create(Session session, IVariantEncoder codec, bool activate) {
                Item = new MonitoredItem {
                    Handle = this,
                    DisplayName = Template.DisplayName,
                    AttributeId = (uint)Template.AttributeId.GetValueOrDefault(
                        (NodeAttribute)Attributes.Value),
                    IndexRange = Template.IndexRange,
                    RelativePath = Template.RelativePath?
                                .ToRelativePath(session.MessageContext)?
                                .Format(session.NodeCache.TypeTree),
                    MonitoringMode = activate
                        ? Template.MonitoringMode.ToStackType().
                            GetValueOrDefault(Opc.Ua.MonitoringMode.Reporting)
                        : Opc.Ua.MonitoringMode.Disabled,
                    StartNodeId = Template.StartNodeId.ToNodeId(session.MessageContext),
                    QueueSize = Template.QueueSize.GetValueOrDefault(1),
                    SamplingInterval = (int)Template.SamplingInterval.
                        GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalMilliseconds,
                    DiscardOldest = !Template.DiscardNew.GetValueOrDefault(false),
                    Filter =
                        Template.DataChangeFilter.ToStackModel() ??
                        codec.Decode(Template.EventFilter, true) ??
                        ((MonitoringFilter)Template.AggregateFilter
                            .ToStackModel(session.MessageContext))
                };
            }

            /// <summary>
            /// Add the monitored item identifier of the triggering item.
            /// </summary>
            /// <param name="id"></param>
            internal void AddTriggerLink(uint? id) {
                if (id != null) {
                    _newTriggers.Add(id.Value);
                }
            }

            /// <summary>
            /// Merge with desired state
            /// </summary>
            /// <param name="model"></param>
            internal bool MergeWith(MonitoredItemWrapper model) {

                if (model == null || Item == null) {
                    return false;
                }

                var changes = false;
                if (Template.SamplingInterval.GetValueOrDefault(TimeSpan.FromSeconds(1)) !=
                    model.Template.SamplingInterval.GetValueOrDefault(TimeSpan.FromSeconds(1))) {
                    _logger.LogDebug("{item}: Changing sampling interval from {old} to {new}",
                        this, Template.SamplingInterval.GetValueOrDefault(
                            TimeSpan.FromSeconds(1)).TotalMilliseconds,
                        model.Template.SamplingInterval.GetValueOrDefault(
                            TimeSpan.FromSeconds(1)).TotalMilliseconds);
                    Template.SamplingInterval = model.Template.SamplingInterval;
                    Item.SamplingInterval =
                        (int)Template.SamplingInterval.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalMilliseconds;
                    changes = true;
                }
                if (Template.DiscardNew.GetValueOrDefault(false) !=
                        model.Template.DiscardNew.GetValueOrDefault()) {
                    _logger.LogDebug("{item}: Changing discard new mode from {old} to {new}",
                        this, Template.DiscardNew.GetValueOrDefault(false),
                        model.Template.DiscardNew.GetValueOrDefault(false));
                    Template.DiscardNew = model.Template.DiscardNew;
                    Item.DiscardOldest = !Template.DiscardNew.GetValueOrDefault(false);
                    changes = true;
                }
                if (Template.QueueSize.GetValueOrDefault(1) !=
                    model.Template.QueueSize.GetValueOrDefault(1)) {
                    _logger.LogDebug("{item}: Changing queue size from {old} to {new}",
                        this, Template.QueueSize.GetValueOrDefault(1),
                        model.Template.QueueSize.GetValueOrDefault(1));
                    Template.QueueSize = model.Template.QueueSize;
                    Item.QueueSize = Template.QueueSize.GetValueOrDefault(1);
                    changes = true;
                }
                if (Template.MonitoringMode.GetValueOrDefault(Publisher.Models.MonitoringMode.Reporting) !=
                    model.Template.MonitoringMode.GetValueOrDefault(Publisher.Models.MonitoringMode.Reporting)) {
                    _logger.LogDebug("{item}: Changing monitoring mode from {old} to {new}",
                        this, Template.MonitoringMode.GetValueOrDefault(Publisher.Models.MonitoringMode.Reporting),
                        model.Template.MonitoringMode.GetValueOrDefault(Publisher.Models.MonitoringMode.Reporting));
                    Template.MonitoringMode = model.Template.MonitoringMode;
                    _modeChange = Template.MonitoringMode.GetValueOrDefault(Publisher.Models.MonitoringMode.Reporting);
                }
                if (Template.DisplayName != model.Template.DisplayName) {
                    Template.DisplayName = model.Template.DisplayName;
                    Item.DisplayName = Template.DisplayName;
                    changes = true;
                }

                // TODO
                // monitoredItem.Filter = monitoredItemInfo.Filter?.ToStackType();
                return changes;
            }

            /// <summary>
            /// Get triggering configuration changes for this item
            /// </summary>
            /// <param name="addLinks"></param>
            /// <param name="removeLinks"></param>
            /// <returns></returns>
            internal bool GetTriggeringLinks(out IEnumerable<uint> addLinks,
                out IEnumerable<uint> removeLinks) {
                var remove = _triggers.Except(_newTriggers).ToList();
                var add = _newTriggers.Except(_triggers).ToList();
                _triggers = _newTriggers;
                _newTriggers = new HashSet<uint>();
                addLinks = add;
                removeLinks = remove;
                if (add.Count > 0 || remove.Count > 0) {
                    _logger.LogDebug("{item}: Adding {add} links and removing {remove} links",
                        this, add.Count, remove.Count);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Get any changes in the monitoring mode
            /// </summary>
            /// <returns></returns>
            internal Opc.Ua.MonitoringMode? GetMonitoringModeChange() {
                var change = _modeChange.ToStackType();
                _modeChange = null;
                return Item.MonitoringMode == change ? null : change;
            }

            /// <summary>
            /// Update status
            /// </summary>
            /// <param name="listener"></param>
            /// <param name="subscriptionId"></param>
            /// <param name="subscription"></param>
            internal void UpdateStatus(ISubscriptionListener listener,
                string subscriptionId, Subscription subscription) {
                listener.OnMonitoredItemStatusChange(subscriptionId, subscription,
                    Template.Id, Item?.NodeClass != null && Item.NodeClass != Opc.Ua.NodeClass.Variable,
                    Item?.Status?.ClientHandle,
                    ServerId == 0u ? null : ServerId,
                    Item.Status?.Error
                );
            }

            private HashSet<uint> _newTriggers = new HashSet<uint>();
            private HashSet<uint> _triggers = new HashSet<uint>();
            private Publisher.Models.MonitoringMode? _modeChange;
            private readonly ILogger _logger;
        }

        private readonly ILogger _logger;
        // TODO - check if we still need this list here
        private readonly ConcurrentDictionary<string, SubscriptionWrapper> _subscriptions =
            new ConcurrentDictionary<string, SubscriptionWrapper>();
        private readonly ISessionManager _sessionManager;
        private readonly IVariantEncoderFactory _codec;
    }
}