// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Services {
    using Microsoft.IIoT.Platform.Discovery.Models;
    using Microsoft.IIoT.Platform.Discovery;
    using Microsoft.IIoT.Platform.Core.Models;
    using Microsoft.IIoT.Exceptions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Endpoint registry.
    /// </summary>
    public sealed class EndpointRegistry : IEndpointRegistry, IApplicationEndpointRegistry,
        IEndpointBulkProcessor, IApplicationRegistryListener, IDisposable {

        /// <summary>
        /// Create endpoint registry
        /// </summary>
        /// <param name="database"></param>
        /// <param name="broker"></param>
        /// <param name="logger"></param>
        /// <param name="events"></param>
        public EndpointRegistry(IEndpointRepository database,
            IDiscoveryEventBroker<IEndpointRegistryListener> broker, ILogger logger,
            IDiscoveryEvents<IApplicationRegistryListener> events = null) {

            _database = database ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Register for application registry events
            _unregister = events?.Register(this);
        }

        /// <inheritdoc/>
        public void Dispose() {
            _unregister?.Invoke();
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoModel> GetEndpointAsync(string endpointId,
            CancellationToken ct) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            var endpoint = await _database.FindAsync(endpointId, ct).ConfigureAwait(false);
            if (endpoint == null) {
                throw new ResourceNotFoundException("Endpoint not found");
            }
            return endpoint;
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> ListEndpointsAsync(string continuation,
            int? pageSize, CancellationToken ct) {
            return await _database.QueryAsync(null,
                continuation, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EndpointInfoListModel> QueryEndpointsAsync(
            EndpointInfoQueryModel model, int? pageSize,
            CancellationToken ct) {
            return await _database.QueryAsync(model, null, pageSize, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task OnApplicationNewAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationUpdatedAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationLostAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            // TODO: Loose all endpoints
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationFoundAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnApplicationDeletedAsync(OperationContextModel context,
            ApplicationInfoModel application) {
            // Get all endpoint registrations and for each one, call delete, if failure,
            // stop half way and throw and do not complete.
            var endpoints = await GetEndpointsAsync(application.ApplicationId).ConfigureAwait(false);
            foreach (var endpoint in endpoints) {
                await _database.DeleteAsync(endpoint.Id,
                    ep => Task.FromResult(true)).ConfigureAwait(false);
                if (!endpoint.IsLost()) {
                    await _broker.NotifyAllAsync(l => l.OnEndpointLostAsync(context,
                        endpoint)).ConfigureAwait(false);
                }
                await _broker.NotifyAllAsync(l => l.OnEndpointDeletedAsync(context,
                    endpoint)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EndpointInfoModel>> GetApplicationEndpointsAsync(
            string applicationId, bool includeDeleted, CancellationToken ct) {
            // Include non-visible twins if the application itself is not visible. Otherwise omit.
            var endpoints = await GetEndpointsAsync(applicationId,
                includeDeleted ? (EntityVisibility?)null : EntityVisibility.Found,
                    ct).ConfigureAwait(false);
            return endpoints;
        }

        /// <inheritdoc/>
        public async Task ProcessDiscoveryEventsAsync(IEnumerable<EndpointInfoModel> newEndpoints,
            DiscoveryContextModel context, string discovererId, string applicationId) {

            if (newEndpoints == null) {
                throw new ArgumentNullException(nameof(newEndpoints));
            }

            var operationContext = context.Context.Validate();
            var found = newEndpoints.ToList();

            found.ForEach(x => {
                x.ApplicationId = applicationId;
                x.DiscovererId = discovererId;
                x.SetEndpointId();
            });

            var existing = Enumerable.Empty<EndpointInfoModel>();
            if (!string.IsNullOrEmpty(applicationId)) {
                // Merge with existing endpoints of the application
                existing = await GetEndpointsAsync(applicationId).ConfigureAwait(false);
            }

            var remove = new HashSet<EndpointInfoModel>(existing,
                EndpointInfoModelEx.Logical);
            var add = new HashSet<EndpointInfoModel>(found,
                EndpointInfoModelEx.Logical);
            var unchange = new HashSet<EndpointInfoModel>(existing,
                EndpointInfoModelEx.Logical);
            var change = new HashSet<EndpointInfoModel>(found,
                EndpointInfoModelEx.Logical);

            unchange.IntersectWith(add);
            change.IntersectWith(remove);
            remove.ExceptWith(found);
            add.ExceptWith(existing);

            var added = 0;
            var updated = 0;
            var unchanged = 0;
            var lost = 0;

            if (!(context.RegisterOnly ?? false)) {
                // Remove or disable an endpoint
                foreach (var item in remove) {
                    try {
                        var wasLost = false;
                        var wasPatched = false;
                        var endpoint = await _database.UpdateAsync(item.Id, existing => {
                            wasLost = existing.IsLost();
                            item.Updated = operationContext;
                            item.SetAsLost();
                            wasPatched = existing.Patch(item, out existing);
                            return Task.FromResult(wasPatched);
                        }).ConfigureAwait(false);

                        if (wasPatched) {
                            await _broker.NotifyAllAsync(
                                l => l.OnEndpointUpdatedAsync(operationContext, endpoint)).ConfigureAwait(false);
                            updated++;
                        }
                        else {
                            unchanged++;
                        }
                        if (wasLost) {
                            await _broker.NotifyAllAsync(
                                l => l.OnEndpointLostAsync(operationContext, endpoint)).ConfigureAwait(false);
                            lost++;
                        }
                    }
                    catch (ResourceNotFoundException) {
                        unchanged++; // Can happen if endpoint is already gone
                    }
                    catch (Exception ex) {
                        unchanged++;
                        _logger.LogError(ex, "Exception while disabling endpoint.");
                    }
                }
            }

            // Update endpoints that were found or changed
            foreach (var exists in unchange) {
                try {
                    var wasFound = false;
                    var wasPatched = false;
                    // Get the new one we will patch over the existing one...
                    var patch = change.First(x =>
                        EndpointInfoModelEx.Logical.Equals(x, exists));
                    var endpoint = await _database.UpdateAsync(exists.Id, existing => {
                        patch.SetAsFound();
                        patch.Updated = operationContext;
                        wasFound = existing.IsLost();
                        wasPatched = existing.Patch(patch, out existing);
                        return Task.FromResult(wasPatched);
                    }).ConfigureAwait(false);
                    if (wasPatched) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointUpdatedAsync(operationContext, endpoint)).ConfigureAwait(false);
                        updated++;
                    }
                    else {
                        unchanged++;
                    }
                    if (wasFound) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointFoundAsync(operationContext, endpoint)).ConfigureAwait(false);
                    }
                }
                catch (ResourceNotFoundException) {
                    unchanged++; // Can happen if endpoint is gone
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.LogError(ex, "Exception during update of endpoint during discovery.");
                }
            }

            // Add endpoint
            foreach (var item in add) {
                try {
                    var wasFound = false;
                    var wasPatched = false;
                    var wasAdded = false;
                    var endpoint = await _database.AddOrUpdateAsync(item.Id, existing => {
                        item.SetAsFound();
                        item.Updated = operationContext;

                        wasAdded = existing == null;
                        wasFound = existing?.IsLost() ?? true;
                        wasPatched = existing.Patch(item, out var addOrUpdated);
                        if (wasAdded) {
                            addOrUpdated.Created = operationContext;
                        }
                        return Task.FromResult(addOrUpdated);
                    }).ConfigureAwait(false);
                    if (wasAdded) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointNewAsync(operationContext, endpoint)).ConfigureAwait(false);
                        added++;
                    }
                    else if (wasPatched) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointUpdatedAsync(operationContext, endpoint)).ConfigureAwait(false);
                        updated++;
                    }
                    if (wasFound) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointFoundAsync(operationContext, endpoint)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.LogError(ex, "Exception adding endpoint from discovery.");
                }
            }

            if (added != 0 || lost != 0) {
                _logger.LogInformation("processed endpoint results: {added} endpoints added, {updated} " +
                    "updated, {removed} removed or disabled, and {unchanged} unchanged.",
                    added, updated, lost, unchanged);
            }
        }

        /// <summary>
        /// Get all endpoints for application id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="visibility"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EndpointInfoModel>> GetEndpointsAsync(
            string applicationId, EntityVisibility? visibility = null, CancellationToken ct = default) {
            return await _database.QueryAllAsync(new EndpointInfoQueryModel {
                Visibility = visibility,
                ApplicationId = applicationId
            }, ct).ConfigureAwait(false);
        }

        private readonly IDiscoveryEventBroker<IEndpointRegistryListener> _broker;
        private readonly IEndpointRepository _database;
        private readonly Action _unregister;
        private readonly ILogger _logger;
    }
}
