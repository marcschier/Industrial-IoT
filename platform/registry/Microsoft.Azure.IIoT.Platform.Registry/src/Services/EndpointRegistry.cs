// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Services {
    using Microsoft.Azure.IIoT.Platform.Registry.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Exceptions;
    using Serilog;
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
        /// <param name="certificates"></param>
        /// <param name="events"></param>
        public EndpointRegistry(IEndpointRepository database,
            IRegistryEventBroker<IEndpointRegistryListener> broker,
            ICertificateServices<EndpointInfoModel> certificates, ILogger logger,
            IRegistryEvents<IApplicationRegistryListener> events = null) {

            _database = database ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _certificates = certificates ?? throw new ArgumentNullException(nameof(certificates));
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
        public async Task UpdateEndpointAsync(string endpointId,
            EndpointInfoUpdateModel request, CancellationToken ct) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var context = request.Context.Validate();

            var endpoint = await _database.UpdateAsync(endpointId, existing => {
                if (existing.GenerationId != request.GenerationId) {
                    throw new ResourceOutOfDateException("Generation id no match");
                }
                if (request.ActivationState != null) {
                    existing.ActivationState = request.ActivationState.Value;
                }
                existing.Updated = context;
                return Task.FromResult(true);
            }, ct).ConfigureAwait(false);

            // Send update to through broker
            await _broker.NotifyAllAsync(l => l.OnEndpointUpdatedAsync(context,
                endpoint)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task OnApplicationNewAsync(RegistryOperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnApplicationUpdatedAsync(RegistryOperationContextModel context,
            ApplicationInfoModel application) {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OnApplicationDeletedAsync(RegistryOperationContextModel context,
            string applicationId, ApplicationInfoModel application) {
            // Get all endpoint registrations and for each one, call delete, if failure,
            // stop half way and throw and do not complete.
            var endpoints = await GetEndpointsAsync(applicationId).ConfigureAwait(false);
            foreach (var endpoint in endpoints) {
                await _database.DeleteAsync(endpoint.Id, 
                    ep => Task.FromResult(true)).ConfigureAwait(false);
                await _broker.NotifyAllAsync(l => l.OnEndpointDeletedAsync(context,
                    endpoint.Id, endpoint)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EndpointInfoModel>> GetApplicationEndpoints(
            string applicationId, bool includeDeleted, bool filterInactiveTwins,
            CancellationToken ct) {
            // Include non-visible twins if the application itself is not visible. Otherwise omit.
            var endpoints = await GetEndpointsAsync(applicationId, 
                includeDeleted ? (EntityVisibility?)null : EntityVisibility.Found, 
                    ct).ConfigureAwait(false);
            if (!filterInactiveTwins) {
                return endpoints;
            }
            return endpoints.Where(e => e.IsActivated());
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
            var disabled = 0;

            if (!(context.RegisterOnly ?? false)) {
                // Remove or disable an endpoint
                foreach (var item in remove) {
                    try {
                        var wasDisabled = false;
                        var endpoint = await _database.UpdateAsync(item.Id, existing => {
                            wasDisabled = existing.IsNotSeen();
                            existing.Patch(item);
                            existing.Updated = operationContext;
                            existing.SetNotSeen();
                            return Task.FromResult(true);
                        }).ConfigureAwait(false);

                        if (wasDisabled) {
                            await _broker.NotifyAllAsync(
                                l => l.OnEndpointUpdatedAsync(operationContext, endpoint)).ConfigureAwait(false);
                            disabled++;
                        }
                        else {
                            unchanged++;
                        }
                    }
                    catch (Exception ex) {
                        unchanged++;
                        _logger.Error(ex, "Exception while disabling endpoint.");
                    }
                }
            }

            // Update endpoints that were disabled
            foreach (var exists in unchange) {
                try {
                    var wasUpdated = false;
                    // Get the new one we will patch over the existing one...
                    var patch = change.First(x =>
                        EndpointInfoModelEx.Logical.Equals(x, exists));
                    patch.ActivationState = exists.ActivationState;
                    var endpoint = await _database.UpdateAsync(exists.Id, (Func<EndpointInfoModel, Task<bool>>)(existing => {
                        wasUpdated = existing.IsNotSeen();
                        existing.Patch(patch);
                        existing.SetAsFound();
                        existing.Updated = operationContext;
                        return Task.FromResult(true);
                    })).ConfigureAwait(false);
                    if (wasUpdated) {
                        await _broker.NotifyAllAsync(
                            l => l.OnEndpointUpdatedAsync(operationContext,
                                endpoint)).ConfigureAwait(false);
                        updated++;
                    }
                    else {
                        unchanged++;
                    }
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception during update of endpoint during discovery.");
                }
            }

            // Add endpoint
            foreach (var item in add) {
                try {
                    var update = false;
                    var endpoint = await _database.AddOrUpdateAsync(item.Id, existing => {
                        update = existing != null;
                        var addOrUpdated = existing.Patch(item);
                        addOrUpdated.SetAsFound();
                        addOrUpdated.Updated = operationContext;
                        if (!update) {
                            addOrUpdated.Created = operationContext;
                        }
                        return Task.FromResult(addOrUpdated);
                    }).ConfigureAwait(false);
                    if (update) {
                        await _broker.NotifyAllAsync(l => l.OnEndpointUpdatedAsync(
                            operationContext, endpoint)).ConfigureAwait(false);
                        updated++;
                    }
                    else {
                        await _broker.NotifyAllAsync(l => l.OnEndpointNewAsync(
                            operationContext, endpoint)).ConfigureAwait(false);
                        added++;
                    }
                }
                catch (Exception ex) {
                    unchanged++;
                    _logger.Error(ex, "Exception adding endpoint from discovery.");
                }
            }

            if (added != 0 || disabled != 0) {
                _logger.Information("processed endpoint results: {added} endpoints added, {updated} " +
                    "updated, {removed} removed or disabled, and {unchanged} unchanged.",
                    added, updated, disabled, unchanged);
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

        private readonly ICertificateServices<EndpointInfoModel> _certificates;
        private readonly IRegistryEventBroker<IEndpointRegistryListener> _broker;
        private readonly IEndpointRepository _database;
        private readonly Action _unregister;
        private readonly ILogger _logger;
    }
}
